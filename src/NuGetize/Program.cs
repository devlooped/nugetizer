using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using ColoredConsole;

namespace NuGetize
{
    class Program
    {
        static string directoryProject;

        static int Main(string[] args)
        {
            directoryProject = !File.Exists("Directory.Build.targets") ?
                "Directory.Build.targets" :
                !File.Exists("Directory.Build.props") ?
                "Directory.Build.props" :
                null;

            if (directoryProject == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Current directory contains both Directory.Build.props and Directory.Build.targets file. This is not supported.");
                Console.ResetColor();
                return -1;
            }

            var binlog = false;
#if DEBUG
            binlog = true;
#endif

            var runner = new CommandLineBuilder(
                new RootCommand
                {
                    CommandHandler.Create<bool, string>(Execute),
                    new Option("--bl", "Emit a binary log.") { Argument = new Argument<bool>(name: "bl", getDefaultValue: () => binlog || Debugger.IsAttached) },
                    new Option<string>(new [] { "-v", "--version" }, $"Version of NuGetizer to use. Defaults to {ThisAssembly.Project.Version}, included with the tool."),
                })
                .UseHelp()
                .UseParseDirective()
                .UseSuggestDirective()
                .UseParseErrorReporting()
                .UseExceptionHandler()
                .UseDebugDirective()
                .UseDefaults()
                .Build();

            return runner.Invoke(args);
        }

        static int Execute(bool binlog = false, string version = ThisAssembly.Project.Version)
        {
#if DEBUG
            binlog = true;
#endif
            binlog |= Debugger.IsAttached;

            try
            {
                var tooldir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var project = "";
                var bl = binlog ? $"-bl:\"{Directory.GetCurrentDirectory()}\\msbuild.binlog;ProjectImports=None\"" : "";

                var projects = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.*proj").ToList();

                // TODO: allow specifying which project.
                if (projects.Count > 1)
                    project = projects[0];

                File.Copy(Path.Combine(tooldir, "NuGetize.Tweaks.targets"), "NuGetize.Tweaks.targets", true);
                File.WriteAllText(directoryProject,
                    File.ReadAllText(Path.Combine(tooldir, "NuGetize.Build.targets"))
                        .Replace("$$tooldir$$", tooldir)
                        .Replace("$$version$$", ThisAssembly.Project.Version));

                if (!Execute(DotnetMuxer.Path.FullName, $"restore {project} -nologo {bl}"))
                    return -1;

                // Optimize the "build" so that it doesn't actually do a full compile if possible.
                if (!Execute(DotnetMuxer.Path.FullName, $"msbuild {project} -nologo {bl} -t:\"GetPackageContents;Pack\" -p:SkipCompilerExecution=true -p:DesignTimeBuild=true -p:DesignTimeSilentResolution=true -p:ResolveAssemblyReferencesSilent=true"))
                    return -1;

                var items = XDocument.Load("NuGetizer.items");
                var metadata = items.Root.Descendants("PackageMetadata").FirstOrDefault();

                if (metadata != null)
                {
                    ColorConsole.WriteLine($"Package: {Path.GetFileName(metadata.Element("NuPkg").Value)}".Yellow());

                    var width = metadata.Elements()
                        .Select(x => x.Name.LocalName.Length)
                        .OrderByDescending(x => x)
                        .First();

                    foreach (var md in metadata.Elements().Where(x => x.Name != "Id" && x.Name != "Nuspec").OrderBy(x => x.Name.LocalName))
                    {
                        ColorConsole.WriteLine($"\t{md.Name.LocalName.PadRight(width)}: {md.Value}");
                    }
                }
                else
                {
                    ColorConsole.WriteLine($"Project is not packable. Make sure it has a valid PackageId.".Red());
                    return -1;
                }

                ColorConsole.WriteLine($"Contents:".Yellow());

                var contents = items.Root.Descendants("PackageContent")
                    .Where(x => x.Element("PackagePath") != null)
                    .Select(x => x.Element("PackagePath").Value)
                    .OrderBy(x => x);

                Render(contents.ToList(), 0, 0, "");

                Console.WriteLine();
                ColorConsole.WriteLine("Nuspec output: ", new FileInfo(metadata.Attribute("Include").Value + ".nuspec").FullName.Yellow());
                ColorConsole.WriteLine("MSBuild items: ", new FileInfo("NuGetizer.items").FullName.Yellow());

                return 0;
            }
            finally
            {
                if (File.Exists(directoryProject))
                    File.Delete(directoryProject);
                if (File.Exists("NuGetize.Tweaks.targets"))
                    File.Delete("NuGetize.Tweaks.targets");
            }
        }

        static int Render(IList<string> files, int index, int level, string path)
        {
            while (index < files.Count)

            {
                var file = files[index];
                var dir = Path.GetDirectoryName(file);
                var paths = dir.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

                if (!string.Join(Path.DirectorySeparatorChar, paths).StartsWith(path) ||
                    paths.Length < level)
                    return index;

                if (paths.Length > level)
                {
                    ColorConsole.Write(new string(' ', (level + 1) * 2));
                    if (level == 0)
                        ColorConsole.Write("/".Gray());

                    ColorConsole.WriteLine(paths[level].Green(), "/".Gray());
                    index = Render(files, index, level + 1, string.Join(Path.DirectorySeparatorChar, paths[..(level + 1)]));
                }
                else
                {
                    Console.Write(new string(' ', (level + 1) * 2));
                    if (level == 0)
                        ColorConsole.Write("/".Gray());

                    ColorConsole.WriteLine(Path.GetFileName(file).White());
                    index++;
                }
            }

            return index;
        }

        static bool Execute(string program, string arguments)
        {
            var info = new ProcessStartInfo(program, arguments)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var proc = System.Diagnostics.Process.Start(info);
            Console.Out.Write(proc.StandardOutput.ReadToEnd());
            Console.Error.Write(proc.StandardError.ReadToEnd());

            if (!proc.WaitForExit(5000))
            {
                proc.Kill();
                return false;
            }

            return proc.ExitCode == 0;
        }
    }
}
