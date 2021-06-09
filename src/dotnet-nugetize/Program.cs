using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using ColoredConsole;
using Mono.Options;

namespace NuGetize
{
    class Program
    {
        bool binlog = Debugger.IsAttached;
        bool debug = Debugger.IsAttached && Environment.GetEnvironmentVariable("DEBUG_NUGETIZER") != "0";
        bool quiet = false;
        string items;
        List<string> extra;

        static int Main(string[] args)
        {
            return new Program().Run(args);
        }

        int Run(string[] args)
        {
            var version = false;
            var help = false;

            var options = new OptionSet
            {
                { "?|h|help", "Display this help.", h => help = h != null },
                { "b|bl|binlog", "Generate binlog.", b => binlog = b != null },
                { "d|debug", "Debug nugetizer tasks.", d => debug = d != null, true },
                { "q|quiet", "Don't render MSBuild output.", q => quiet = q != null },
                { "i|items:", "MSBuild items file path containing full package contents metadata.", i => items = Path.GetFullPath(i) },
                { "version", "Render tool version and copyright information.", v => version = v != null },
            }
            .Add("  [msbuild args]             Examples: ")
            .Add("                             - Automatically restore: -r")
            .Add("                             - Set normal MSBuild verbosity: -v:n")
            .Add("                             - Build configuration: -p:Configuration=Release");

            extra = options.Parse(args);

            if (version)
            {
                Console.WriteLine($"{ThisAssembly.Project.Product} version {ThisAssembly.Project.Version}+{ThisAssembly.Project.RepositorySha}");
                Console.WriteLine($"{ThisAssembly.Project.Copyright}");
            }
            else
            {
                extra.Add("-nologo");
            }

            if (binlog)
            {
                extra.Add($"-bl:\"msbuild.binlog;ProjectImports=None\"");
            }

            // Built-in args we always pass to MSBuild
            extra.AddRange(new[]
            {
                // Avoids contention when writing to the dotnet-nugetize MSBuild items file
                "-m:1",
                // Enable retrieving source link info if supported by the project
                "-p:EnableSourceLink=true",
                "-p:EnableSourceControlManagerQueries=true",
                // Allow nuspec inspection after nugetizing
                "-p:EmitNuspec=true",
                // Never emit the actual pkg since that would be the same as just running dotnet pack
                "-p:EmitPackage=false",
                // DTB arguments that speed-up the execution
                "-p:SkipCompilerExecution=true",
                "-p:DesignTimeBuild=true",
                "-p:DesignTimeSilentResolution=true",
                "-p:ResolveAssemblyReferencesSilent=true"
            });

            if (help)
            {
                Console.WriteLine($"Usage: {ThisAssembly.Project.ToolCommandName} [options] [msbuild args]");
                options.WriteOptionDescriptions(Console.Out);
                return 0;
            }

            return Execute();
        }

        int Execute()
        {
            var tooldir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var invalidChars = Path.GetInvalidPathChars();
            var project = extra.Where(arg => arg.IndexOfAny(invalidChars) == -1).FirstOrDefault(arg => File.Exists(arg)) ?? "";
            var file = items ?? Path.GetTempFileName();

            if (string.IsNullOrEmpty(project))
            {
                var projects = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.*proj").ToList();
                var solutions = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.sln").ToList();

                // TODO: allow specifying which project.
                if (solutions.Count == 1)
                    project = solutions[0];
                else if (projects.Count > 1)
                    project = projects[0];
            }
            else
            {
                extra.Remove(project);
            }

            if (File.Exists(file))
                File.Delete(file);

            // Optimize the "build" so that it doesn't actually do a full compile if possible.
            var contentsOnly = false;
            if (!Execute(DotnetMuxer.Path.FullName, $"msbuild {project} {string.Join(' ', extra)} -p:dotnet-nugetize=\"{file}\" -t:\"GetPackageContents;Pack\""))
                return -1;

            if (!File.Exists(file))
            {
                // Re-run requesting contents only.
                if (!Execute(DotnetMuxer.Path.FullName, $"msbuild {project} {string.Join(' ', extra)} -p:dotnet-nugetize-contents=true -p:dotnet-nugetize=\"{file}\" -t:\"GetPackageContents\""))
                    return -1;

                contentsOnly = true;
            }

            var doc = XDocument.Load(file);

            if (contentsOnly)
            {
                ColorConsole.WriteLine($"Project {project} is not packable, rendering its contributed package contents.".Yellow());

                var dependencies = doc.Root.Descendants("PackageContent")
                    .Where(x =>
                        "Dependency".Equals(x.Element("PackFolder")?.Value, StringComparison.OrdinalIgnoreCase))
                    .Distinct(AnonymousComparer.Create<XElement>(x =>
                        x.Attribute("Include").Value + "|" +
                        x.Element("Version").Value + "|" +
                        x.Element("TargetFramework").Value))
                    .OrderBy(x => x.Element("TargetFramework").Value)
                    .ThenBy(x => x.Attribute("Include").Value)
                    .ToList();

                if (dependencies.Count > 0)
                {
                    ColorConsole.WriteLine($"  Dependencies:".Yellow());
                    foreach (var group in dependencies.GroupBy(x => x.Element("TargetFramework").Value))
                    {
                        ColorConsole.WriteLine("    ", group.Key.Green());
                        foreach (var dependency in group)
                        {
                            ColorConsole.WriteLine("      ", dependency.Attribute("Include").Value.White(), $", {dependency.Element("Version").Value}".Gray());
                        }
                    }
                }

                ColorConsole.WriteLine($"  Contents:".Yellow());

                var contents = doc.Root.Descendants("PackageContent")
                    .Where(x => x.Element("PackagePath") != null)
                    .Distinct(AnonymousComparer.Create<XElement>(x => x.Element("PackagePath").Value))
                    .OrderBy(x => Path.GetDirectoryName(x.Element("PackagePath").Value))
                    .ThenBy(x => x.Element("PackagePath").Value);

                Render(contents.ToList(), 0, 0, "");
                Console.WriteLine();
            }
            else
            {
                var foundPackage = false;

                foreach (var metadata in doc.Root.Descendants("PackageMetadata")
                    .Distinct(AnonymousComparer.Create<XElement>(
                        (x, y) => x.Element("PackageId")?.Value == y.Element("PackageId")?.Value,
                        x => x.Element("PackageId")?.Value.GetHashCode() ?? 0)))
                {
                    var packageId = metadata.Element("PackageId").Value;
                    if (string.IsNullOrEmpty(packageId))
                        continue;

                    foundPackage = true;
                    ColorConsole.WriteLine($"Package: {Path.GetFileName(metadata.Element("NuPkg").Value)}".Yellow());
                    ColorConsole.WriteLine($"         {metadata.Element("Nuspec").Value}".Yellow());

                    var width = metadata.Elements()
                        .Select(x => x.Name.LocalName.Length)
                        .OrderByDescending(x => x)
                        .First();

                    foreach (var md in metadata.Elements()
                        .Where(x =>
                            x.Name != "PackageId" &&
                            x.Name != "Nuspec" &&
                            x.Name != "NuPkg")
                        .OrderBy(x => x.Name.LocalName))
                    {
                        ColorConsole.WriteLine($"    {md.Name.LocalName.PadRight(width)}: ", md.Value.White());
                    }

                    var dependencies = doc.Root.Descendants("PackageContent")
                        .Where(x =>
                            "Dependency".Equals(x.Element("PackFolder")?.Value, StringComparison.OrdinalIgnoreCase) &&
                            packageId == x.Element("PackageId")?.Value)
                        .Distinct(AnonymousComparer.Create<XElement>(x =>
                            x.Attribute("Include").Value + "|" +
                            x.Element("Version").Value + "|" +
                            x.Element("TargetFramework").Value))
                        .OrderBy(x => x.Element("TargetFramework").Value)
                        .ThenBy(x => x.Attribute("Include").Value)
                        .ToList();

                    if (dependencies.Count > 0)
                    {
                        ColorConsole.WriteLine($"  Dependencies:".Yellow());
                        foreach (var group in dependencies.GroupBy(x => x.Element("TargetFramework").Value))
                        {
                            ColorConsole.WriteLine("    ", group.Key.Green());
                            foreach (var dependency in group)
                            {
                                ColorConsole.WriteLine("      ", dependency.Attribute("Include").Value.White(), $", {dependency.Element("Version").Value}".Gray());
                            }
                        }
                    }

                    ColorConsole.WriteLine($"  Contents:".Yellow());

                    var contents = doc.Root.Descendants("PackageContent")
                        .Where(x =>
                            x.Element("PackagePath") != null &&
                            x.Element("PackageId")?.Value == packageId)
                        .Distinct(AnonymousComparer.Create<XElement>(x => x.Element("PackagePath").Value))
                        .OrderBy(x => Path.GetDirectoryName(x.Element("PackagePath").Value))
                        .ThenBy(x => x.Element("PackagePath").Value);

                    Render(contents.ToList(), 0, 0, "");
                    Console.WriteLine();
                }

                if (!foundPackage)
                {
                    ColorConsole.WriteLine($"No package content was found.".Red());
                    return -1;
                }
            }

            Console.WriteLine();
            if (items != null)
                ColorConsole.WriteLine("> ", file.Yellow());

            if (binlog)
            {
                // Attempt to open binlog automatically if msbuildlog.com is installed
                try
                {
                    Process.Start(new ProcessStartInfo(Directory.GetCurrentDirectory() + "\\msbuild.binlog")
                    {
                        UseShellExecute = true,
                        WindowStyle = ProcessWindowStyle.Maximized
                    });
                }
                catch { }
            }

            return 0;
        }

        static int Render(IList<XElement> files, int index, int level, string path)
        {
            var normalizedLevelPath = path == "" ? Path.DirectorySeparatorChar.ToString() : (Path.DirectorySeparatorChar + path + Path.DirectorySeparatorChar);
            while (index < files.Count)
            {
                var element = files[index];
                var file = element.Element("PackagePath").Value;
                var dir = Path.GetDirectoryName(file);
                var paths = dir.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
                var normalizeCurrentPath = Path.DirectorySeparatorChar + string.Join(Path.DirectorySeparatorChar, paths) + Path.DirectorySeparatorChar;

                if (!normalizeCurrentPath.StartsWith(normalizedLevelPath) || paths.Length < level)
                    return index;

                if (paths.Length > level)
                {
                    ColorConsole.Write(new string(' ', (level + 1) * 2 + 2));
                    if (level == 0)
                        ColorConsole.Write("/".Gray());

                    ColorConsole.WriteLine(paths[level].Green(), "/".Gray());
                    index = Render(files, index, level + 1, string.Join(Path.DirectorySeparatorChar, paths[..(level + 1)]));
                }
                else
                {
                    Console.Write(new string(' ', (level + 1) * 2 + 2));
                    if (level == 0)
                        ColorConsole.Write("/".Gray());

                    var attributes = new List<string>();
                    var packFolder = element.Element("PackFolder")?.Value;

                    if (packFolder != null &&
                        ("content".Equals(packFolder, StringComparison.OrdinalIgnoreCase) ||
                         "contentFiles".Equals(packFolder, StringComparison.OrdinalIgnoreCase)))
                    {
                        if (element.Element("BuildAction")?.Value is string buildAction)
                            attributes.Add("buildAction=" + buildAction);
                        if (element.Element("CopyToOutput")?.Value is string copyToOutput)
                            attributes.Add("copyToOutput=" + copyToOutput);
                        if (element.Element("Flatten")?.Value is string flatten)
                            attributes.Add("flatten=" + flatten);
                    }

                    ColorConsole.Write(Path.GetFileName(file).White());
                    if (attributes.Count > 0)
                        ColorConsole.Write((" (" + string.Join(',', attributes) + ")").Gray());

                    Console.WriteLine();

                    index++;
                }
            }

            return index;
        }

        bool Execute(string program, string arguments)
        {
            var info = new ProcessStartInfo(program, arguments)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            if (debug)
                info.Environment["DEBUG_NUGETIZER"] = "1";

            var proc = Process.Start(info);
            proc.ErrorDataReceived += (sender, args) => ColorConsole.Write(args.Data.Red());

            var output = new StringBuilder();
            var timedout = false;

            // If process takes too long, start to automatically 
            // render the output.
            while (!proc.WaitForExit(10000))
            {
                timedout = true;
                output.Append(proc.StandardOutput.ReadToEnd());
            }

            if (!timedout)
                output.Append(proc.StandardOutput.ReadToEnd());

            if (!quiet || timedout || proc.ExitCode != 0)
                Console.Out.Write(output.ToString());

            return proc.ExitCode == 0;
        }

        static class AnonymousComparer
        {
            public static IEqualityComparer<T> Create<T>(Func<T, T, bool> equals, Func<T, int> getHashCode)
                => new AnonymousEqualityComparer<T>(equals, getHashCode);

            public static IEqualityComparer<T> Create<T>(Func<T, object> value)
                => new AnonymousEqualityComparer<T>((x, y) => Equals(value(x), value(y)), x => value(x).GetHashCode());
        }

        class AnonymousEqualityComparer<T> : IEqualityComparer<T>
        {
            Func<T, T, bool> equals;
            Func<T, int> getHashCode;

            public AnonymousEqualityComparer(Func<T, T, bool> equals, Func<T, int> getHashCode)
            {
                this.equals = equals;
                this.getHashCode = getHashCode;
            }

            public bool Equals(T x, [AllowNull] T y) => equals(x, y);

            public int GetHashCode([DisallowNull] T obj) => getHashCode(obj);
        }
    }
}
