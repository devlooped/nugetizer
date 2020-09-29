using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using ColoredConsole;

namespace NuGetize
{
    class Program
    {
        static int Main(string[] args)
        {
            var binlog = Debugger.IsAttached || args.Any(arg => arg == "--bl" || arg == "-bl");
            var debug = args.Any(args => args == "--debug");

            if (args.Any(arg => arg == "--version"))
                ColorConsole.WriteLine($"{ThisAssembly.Project.ToolCommandName} version {ThisAssembly.Project.Version}".Green());

            return Execute(binlog, debug);
        }

        static int Execute(bool binlog, bool debug)
        {
            var tooldir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var project = "";
            var bl = binlog ? $"-bl:\"{Directory.GetCurrentDirectory()}\\msbuild.binlog;ProjectImports=None\"" : "";
            var file = Path.Combine(Directory.GetCurrentDirectory(), "dotnet-nugetize.items");

            var projects = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.*proj").ToList();

            // TODO: allow specifying which project.
            if (projects.Count > 1)
                project = projects[0];

            if (!Execute(DotnetMuxer.Path.FullName, $"restore {project} -nologo {bl}", debug))
                return -1;

            if (File.Exists(file))
                File.Delete(file);

            // Optimize the "build" so that it doesn't actually do a full compile if possible.
            if (!Execute(DotnetMuxer.Path.FullName, $"msbuild {project} -m:1 -p:dotnet-nugetize=\"{file}\" -nologo {bl} -t:\"GetPackageContents;Pack\" -p:EmitNuspec=true -p:EmitPackage=false -p:SkipCompilerExecution=true -p:DesignTimeBuild=true -p:DesignTimeSilentResolution=true -p:ResolveAssemblyReferencesSilent=true", debug))
                return -1;

            var items = XDocument.Load(file);
            var foundPackage = false;

            foreach (var metadata in items.Root.Descendants("PackageMetadata")
                .Distinct(AnonymousEqualityComparer.Create<XElement>(
                    (x, y) => x.Element("PackageId")?.Value == y.Element("PackageId")?.Value, 
                    x => x.Element("PackageId")?.Value.GetHashCode() ?? 0)))
            {
                var packageId = metadata.Element("PackageId").Value;
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

                var dependencies = items.Root.Descendants("PackageContent")
                    .Where(x =>
                        "Dependency".Equals(x.Element("Kind")?.Value, StringComparison.OrdinalIgnoreCase) &&
                        x.Element("PackageId")?.Value == packageId)
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

                var contents = items.Root.Descendants("PackageContent")
                    .Where(x => 
                        x.Element("PackagePath") != null && 
                        x.Element("PackageId")?.Value == packageId)
                    .Select(x => x.Element("PackagePath").Value)
                    .OrderBy(x => x);

                RenderContents(contents.ToList(), 0, 0, "");
                Console.WriteLine();
            }

            if (!foundPackage)
            {
                ColorConsole.WriteLine($"No package content was found.".Red());
                return -1;
            }

            Console.WriteLine();
            ColorConsole.WriteLine("Items: ", file.Yellow());

            return 0;
        }

        static int RenderContents(IList<string> files, int index, int level, string path)
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
                    ColorConsole.Write(new string(' ', (level + 1) * 2 + 2));
                    if (level == 0)
                        ColorConsole.Write("/".Gray());

                    ColorConsole.WriteLine(paths[level].Green(), "/".Gray());
                    index = RenderContents(files, index, level + 1, string.Join(Path.DirectorySeparatorChar, paths[..(level + 1)]));
                }
                else
                {
                    Console.Write(new string(' ', (level + 1) * 2 + 2));
                    if (level == 0)
                        ColorConsole.Write("/".Gray());

                    ColorConsole.WriteLine(Path.GetFileName(file).White());
                    index++;
                }
            }

            return index;
        }

        static bool Execute(string program, string arguments, bool debug = false)
        {
            var info = new ProcessStartInfo(program, arguments)
            {
                RedirectStandardOutput = debug,
                RedirectStandardError = debug
            };

            var proc = Process.Start(info);
            if (debug)
            {
                Console.Out.Write(proc.StandardOutput.ReadToEnd());
                Console.Error.Write(proc.StandardError.ReadToEnd());
            }

            if (!proc.WaitForExit(5000))
            {
                proc.Kill();
                return false;
            }

            return proc.ExitCode == 0;
        }

        static class AnonymousEqualityComparer
        {
            public static IEqualityComparer<T> Create<T>(Func<T, T, bool> equals, Func<T, int> getHashCode)
                => new AnonymousEqualityComparer<T>(equals, getHashCode);
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
