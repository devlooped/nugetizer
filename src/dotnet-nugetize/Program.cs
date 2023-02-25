using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Devlooped;
using Mono.Options;
using Spectre.Console;
using Spectre.Console.Rendering;
using static Devlooped.SponsorLink;
using static ThisAssembly.Strings;

namespace NuGetize;

class Program
{
    static readonly Style yellow = new(Color.Yellow);
    static readonly Style errorStyle = new(Color.Red);
    static readonly Style warningStyle = yellow;

    bool binlog = Debugger.IsAttached;
    bool debug = Debugger.IsAttached && Environment.GetEnvironmentVariable("DEBUG_NUGETIZER") != "0";
    bool verbose = false;
    string items;
    List<string> extra;

    static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = Encoding.Unicode;

        var status = SponsorCheck.CheckAsync(Directory.GetCurrentDirectory(), "devlooped", "NuGetizer", "dotnet-nugetize", ThisAssembly.Project.Version);
        var result = new Program().Run(args);

        // No need to check sponsorlink status if we couldn't render useful results.
        if (result == 0)
        {
            var value = await status;
            if (value == null)
                return result;

            switch (value.Value)
            {
                case SponsorStatus.AppMissing:
                    Warning(
                        AppMissing.Header,
                        new Markup(AppMissing.Message1("NuGetizer", "devlooped")),
                        new Grid().AddColumns(2)
                            .AddRow(
                                new Markup(AppMissing.Message2),
                                new Text("https://github.com/apps/sponsorlink",
                                    new Style(Color.Blue, decoration: Decoration.Underline, link: "https://github.com/apps/sponsorlink"))));
                    break;
                case SponsorStatus.NotSponsoring:
                    Warning(
                        NotSponsoring.Header,
                        new Markup(NotSponsoring.Message("NuGetizer")),
                        new Text("https://github.com/sponsors/devlooped",
                            new Style(Color.Blue, decoration: Decoration.Underline, link: "https://github.com/apps/sponsorlink")));
                    break;
                case SponsorStatus.Sponsoring:
                    AnsiConsole.Write(new Markup($":heart_decoration: [grey30]{Sponsoring.Message("NuGetizer", "devlooped")}[/]"));
                    break;
                default:
                    break;
            }
        }

        return result;
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
            { "v|verbose", "Render MSBuild output.", v => verbose = v != null },
            { "i|items:", "MSBuild items file path containing full package contents metadata.", i => items = Path.GetFullPath(i) },
            { "version", "Render tool version and copyright information.", v => version = v != null },
        }
        .Add("  [msbuild args]             Examples: ")
        .Add("                             - Automatically restore: -r")
        .Add("                             - Set normal MSBuild verbosity: -v:n")
        .Add("                             - Set minimal MSBuild verbosity: -v:m")
        .Add("                             - Build configuration: -p:Configuration=Release");

        extra = options.Parse(args);

        if (version)
        {
            AnsiConsole.WriteLine($"{ThisAssembly.Project.Product} version {ThisAssembly.Project.Version}+{ThisAssembly.Project.RepositorySha}");
            AnsiConsole.WriteLine($"{ThisAssembly.Project.Copyright}");
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
            AnsiConsole.WriteLine($"Usage: {ThisAssembly.Project.ToolCommandName} [options] [msbuild args]");
            options.WriteOptionDescriptions(Console.Out);
            return 0;
        }

        return Execute();
    }

    int Execute()
    {
        var tooldir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var invalidChars = Path.GetInvalidPathChars();
        var project = extra.Where(arg => arg.IndexOfAny(invalidChars) == -1).FirstOrDefault(File.Exists) ?? "";
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

        var slnTargets = $"after.{Path.GetFileName(project)}.targets";
        var deleteSlnTargets = false;
        var contentsOnly = false;

        try
        {
            if (project.EndsWith(".sln") && !File.Exists(slnTargets))
            {
                File.WriteAllText(slnTargets, EmbeddedResource.GetContent("after.sln.targets"), Encoding.UTF8);
                deleteSlnTargets = true;
            }

            // Optimize the "build" so that it doesn't actually do a full compile if possible.
            if (!Execute(DotnetMuxer.Path.FullName, $"msbuild {project} {string.Join(' ', extra)} -p:dotnet-nugetize=\"{file}\" -t:\"GetPackageContents;Pack\""))
            {
                // The error might have been caused by us not being able to write our slnTargets
                if (project.EndsWith(".sln") && !deleteSlnTargets)
                    AnsiConsole.Write(new Paragraph($"Solution targets '{slnTargets}' already exists. NuGetizing all projects in the solution is therefore required.", warningStyle));

                if (binlog)
                    TryOpenBinlog();

                return -1;
            }

            if (!File.Exists(file))
            {
                // Re-run requesting contents only.
                if (!Execute(DotnetMuxer.Path.FullName, $"msbuild {project} {string.Join(' ', extra)} -p:dotnet-nugetize-contents=true -p:dotnet-nugetize=\"{file}\" -t:\"GetPackageContents\""))
                {
                    // The error might have been caused by us not being able to write our slnTargets
                    if (project.EndsWith(".sln") && !deleteSlnTargets)
                        AnsiConsole.Write(new Paragraph($"Solution targets '{slnTargets}' already exists. NuGetizing all projects in the solution is therefore required.", warningStyle));

                    if (binlog)
                        TryOpenBinlog();

                    return -1;
                }

                contentsOnly = true;
            }
        }
        finally
        {
            if (File.Exists(slnTargets) && deleteSlnTargets)
                File.Delete(slnTargets);
        }

        if (!File.Exists(file))
        {
            AnsiConsole.Write(new Paragraph("Failed to discover nugetized content.", errorStyle));
            return -1;
        }

        var doc = XDocument.Load(file);
        Tree? root = default;

        if (contentsOnly)
        {
            root = new Tree("[yellow]Package Contents[/]");

            if (project.EndsWith(".sln"))
                AnsiConsole.Write(new Paragraph($"Solution {Path.GetFileName(project)} contains only non-packable project(s), rendering contributed package contents.", errorStyle));
            else
                AnsiConsole.Write(new Paragraph($"Project {Path.GetFileName(project)} is not packable, rendering its contributed package contents.", warningStyle));

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

            AddDependencies(root, dependencies);

            var contents = doc.Root.Descendants("PackageContent")
                .Where(x => x.Element("PackagePath") != null)
                .Distinct(AnonymousComparer.Create<XElement>(x => x.Element("PackagePath").Value))
                .OrderBy(x => Path.GetDirectoryName(x.Element("PackagePath").Value))
                .ThenBy(x => x.Element("PackagePath").Value);

            AddContents(root.AddNode("[yellow]Contents:[/]"), contents.ToList());
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

                var specFile = new FileInfo(metadata.Element("Nuspec").Value).FullName;
                if (specFile.StartsWith(Directory.GetCurrentDirectory()))
                    specFile = specFile.Replace(Directory.GetCurrentDirectory(), "");
                if (specFile.StartsWith(Path.DirectorySeparatorChar) || specFile.StartsWith(Path.AltDirectorySeparatorChar))
                    specFile = "." + specFile;

                var grid = new Grid();
                grid.AddColumn().AddColumn();
                grid.AddRow(new Text("Package", yellow), new Grid().AddColumn()
                    .AddRow($"[yellow]{Path.GetFileName(metadata.Element("NuPkg").Value)}[/]")
                    .AddRow(new Text(specFile,
                        new Style(Color.Blue, decoration: Decoration.Underline, link: specFile))));

                root = new Tree(grid);

                var width = metadata.Elements()
                    .Select(x => x.Name.LocalName.Length)
                    .OrderByDescending(x => x)
                    .First();

                var table = new Grid().AddColumn().AddColumn();
                table.AddRow(new Text("Metadata:", yellow));

                foreach (var md in metadata.Elements()
                    .Where(x =>
                        x.Name != "PackageId" &&
                        x.Name != "Nuspec" &&
                        x.Name != "NuPkg")
                    .OrderBy(x => x.Name.LocalName))
                {
                    table.AddRow(new Text(md.Name.LocalName), new Text(md.Value));
                }

                root.AddNode(table);

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

                AddDependencies(root, dependencies);

                var contents = doc.Root.Descendants("PackageContent")
                    .Where(x =>
                        x.Element("PackagePath") != null &&
                        x.Element("PackageId")?.Value == packageId)
                    .Distinct(AnonymousComparer.Create<XElement>(x => x.Element("PackagePath").Value))
                    .OrderBy(x => Path.GetDirectoryName(x.Element("PackagePath").Value))
                    .ThenBy(x => x.Element("PackagePath").Value);

                AddContents(root.AddNode("[yellow]Contents:[/]"), contents.ToList());
                //Render(contents.ToList(), 0, 0, "");
                //Console.WriteLine();
            }

            if (!foundPackage)
            {
                AnsiConsole.Write(new Paragraph($"No package content was found.", errorStyle));
                return -1;
            }
        }

        if (root != null)
            AnsiConsole.Write(root);

        AnsiConsole.WriteLine();
        if (items != null)
            AnsiConsole.WriteLine($"> [yellow]{file}[/]");

        if (binlog)
            TryOpenBinlog();

        return 0;
    }

    void AddContents(TreeNode node, List<XElement> contents)
    {
        var parents = new Dictionary<string, TreeNode>();
        parents.Add("", node);
        foreach (var element in contents)
        {
            var file = element.Element("PackagePath").Value;
            var dir = Path.GetDirectoryName(file);
            var paths = dir.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            var parent = node;
            // Locate parent node given the relative path
            if (paths.Length > 0)
            {
                for (var i = 0; i < paths.Length; i++)
                {
                    var key = string.Join('/', paths[0..(i + 1)]);
                    if (!parents.TryGetValue(key, out var existing))
                    {
                        parent = parent.AddNode($"[green]{paths[i]}[/]");
                        parents.Add(key, parent);
                    }
                    else
                    {
                        parent = existing;
                    }
                }
            }

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

            if (attributes.Count > 0)
                parent.AddNode($"[white]{Path.GetFileName(file)}[/] [grey]({string.Join(',', attributes)})[/]");
            else
                parent.AddNode($"[white]{Path.GetFileName(file)}[/]");
        }
    }

    void AddDependencies(Tree root, List<XElement> dependencies)
    {
        if (dependencies.Count == 0)
            return;

        var deps = root.AddNode("[yellow]Dependencies:[/]");
        foreach (var group in dependencies.GroupBy(x => x.Element("TargetFramework").Value))
        {
            var tf = deps.AddNode($"[green]{group.Key}[/]");
            foreach (var dependency in group)
            {
                tf.AddNode(Markup.FromInterpolated($"[white]{dependency.Attribute("Include").Value}[/], [grey]{dependency.Element("Version").Value}[/]"));
            }
        }
    }

    static void TryOpenBinlog()
    {
        // Attempt to open binlog automatically if msbuildlog.com is installed
        try
        {
            var process = Process.Start(new ProcessStartInfo(Directory.GetCurrentDirectory() + "\\msbuild.binlog")
            {
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Maximized
            });
            process.WaitForInputIdle();
        }
        catch { }
    }

    bool Execute(string program, string arguments) //=> AnsiConsole.Status().Start("Running MSBuild...", ctx =>
    {
        var info = new ProcessStartInfo(program, arguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        if (debug)
            info.Environment["DEBUG_NUGETIZER"] = "1";

        var proc = Process.Start(info);
        proc.ErrorDataReceived += (sender, args) => AnsiConsole.Write(new Paragraph(args.Data, errorStyle));

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

        if (!verbose && proc.ExitCode == 0)
            return true;

        Console.Out.Write(output.ToString());

        return proc.ExitCode == 0;
    }
    //);

    const int DefaultMaxPause = 4000;

    static DateTime InstallTime { get; } = File.GetCreationTime(Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName);
    static readonly Random rnd = new();

    static void Warning(string header, params IRenderable[] content)
    {
        var grid = new Grid().AddColumn();
        foreach (var c in content)
            grid.AddRow(c);

        var daysOld = (int)DateTime.Now.Subtract(InstallTime).TotalDays;
        if (daysOld == 0)
            // Get some pause from the start.
            daysOld = 1;

        // Turn days pause (starting at 1sec max pause into milliseconds, used for the pause.
        var daysMaxPause = daysOld * 1000;

        // From second day, the max pause will increase from days old until the max pause.
        var pause = rnd.Next(0, Math.Min(daysMaxPause, DefaultMaxPause));

        Thread.Sleep(pause);
        if (pause > 0)
            grid.AddRow(Paused(pause));

        var panel = new Panel(new Padder(grid));
        panel.Header($":warning:  {header} :warning:");
        panel.Border(BoxBorder.Rounded);
        panel.BorderColor(Color.Yellow);

        AnsiConsole.Write(new Padder(panel, new Padding(2, 1)));
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
