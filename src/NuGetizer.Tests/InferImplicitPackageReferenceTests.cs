using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging.StructuredLogger;
using Microsoft.Build.Utilities;
using NuGet.Packaging.Signing;
using NuGetizer.Tasks;
using Xunit;
using Xunit.Abstractions;
using Metadata = System.Collections.Generic.Dictionary<string, string>;

namespace NuGetizer;

public class InferImplicitPackageReferenceTests
{
    readonly ITestOutputHelper output;
    readonly MockBuildEngine engine;

    public InferImplicitPackageReferenceTests(ITestOutputHelper output)
    {
        this.output = output;
        engine = new MockBuildEngine(output);
    }

    [Fact]
    public void when_file_has_no_kind_then_logs_error_code()
    {
        var task = new InferImplicitPackageReference
        {
            BuildEngine = engine,
            PackageReferences = new ITaskItem[]
            {
                new TaskItem("NuGetizer", new Metadata
                {
                    { "Version", "1.0.0" },
                    { "PrivateAssets", "all" },
                }),
                new TaskItem("Devlooped.SponsorLink", new Metadata
                {
                    { "Version", "1.0.0" },
                })
            },
            PackageDependencies = new ITaskItem[]
            {
                new TaskItem("NuGetizer", new Metadata
                {
                    { "ParentPackage", "" },
                }),
                new TaskItem("Devlooped.SponsorLink", new Metadata
                {
                    { "ParentPackage", "" },
                }),
                new TaskItem("NuGetizer/1.0.0", new Metadata
                {
                    { "ParentTarget", "netstandard2.0" },
                    { "ParentPackage", "" },
                }),
                new TaskItem("Devlooped.SponsorLink/1.0.0", new Metadata
                {
                    { "ParentTarget", "netstandard2.0" },
                    { "ParentPackage", "NuGetizer/1.0.0" },
                })
            }
        };

        Assert.True(task.Execute());
        Assert.Empty(task.ImplicitPackageReferences);
    }

    [Fact]
    public void when_reference_is_development_dependency_then_skips_transitive_dependencies()
    {
        var task = new InferImplicitPackageReference
        {
            BuildEngine = engine,
            PackageReferences = new ITaskItem[]
            {
                new TaskItem("Meta", new Metadata
                {
                    { "Version", "1.0.0" },
                    { "PrivateAssets", "all" },
                }),
            },
            PackageDependencies = new ITaskItem[]
            {
                Dependency("Meta/1.0.0"),
                Dependency("Generator/1.0.0", "Meta/1.0.0"),
                Dependency("Microsoft.CSharp/4.7.0", "Generator/1.0.0"),
            },
            PackageDefinitions = new ITaskItem[]
            {
                Definition("Meta", "1.0.0", developmentDependency: true),
                Definition("Generator", "1.0.0", developmentDependency: true),
                Definition("Microsoft.CSharp", "4.7.0"),
            }
        };

        Assert.True(task.Execute());
        Assert.Empty(task.ImplicitPackageReferences);
    }

    [Fact]
    public void when_transitive_is_development_dependency_then_skips_it_and_its_dependencies()
    {
        var task = new InferImplicitPackageReference
        {
            BuildEngine = engine,
            PackageReferences = new ITaskItem[]
            {
                new TaskItem("Library", new Metadata
                {
                    { "Version", "1.0.0" },
                    { "PrivateAssets", "all" },
                }),
            },
            PackageDependencies = new ITaskItem[]
            {
                Dependency("Library/1.0.0"),
                Dependency("Runtime/1.0.0", "Library/1.0.0"),
                Dependency("Generator/1.0.0", "Library/1.0.0"),
                Dependency("Microsoft.CSharp/4.7.0", "Generator/1.0.0"),
            },
            PackageDefinitions = new ITaskItem[]
            {
                Definition("Library", "1.0.0"),
                Definition("Runtime", "1.0.0"),
                Definition("Generator", "1.0.0", developmentDependency: true),
                Definition("Microsoft.CSharp", "4.7.0"),
            }
        };

        Assert.True(task.Execute());
        var inferred = Assert.Single(task.ImplicitPackageReferences);
        Assert.Equal("Runtime", inferred.ItemSpec);
        Assert.Equal("all", inferred.GetMetadata("PrivateAssets"));
    }

    [Fact]
    public void when_no_package_definitions_then_infers_all_transitive_dependencies()
    {
        var task = new InferImplicitPackageReference
        {
            BuildEngine = engine,
            PackageReferences = new ITaskItem[]
            {
                new TaskItem("Meta", new Metadata
                {
                    { "Version", "1.0.0" },
                    { "PrivateAssets", "all" },
                }),
            },
            PackageDependencies = new ITaskItem[]
            {
                Dependency("Meta/1.0.0"),
                Dependency("Generator/1.0.0", "Meta/1.0.0"),
                Dependency("Microsoft.CSharp/4.7.0", "Generator/1.0.0"),
            },
        };

        Assert.True(task.Execute());
        Assert.Equal(
            new[] { "Generator", "Microsoft.CSharp" },
            task.ImplicitPackageReferences.Select(x => x.ItemSpec).OrderBy(x => x));
    }

    static ITaskItem Dependency(string identity, string parent = "")
        => new TaskItem(identity, new Metadata
        {
            { "ParentTarget", "netstandard2.0" },
            { "ParentPackage", parent },
        });

    static ITaskItem Definition(string id, string version, bool developmentDependency = false)
    {
        var path = Path.Combine(Path.GetTempPath(), "nugetizer", nameof(InferImplicitPackageReferenceTests), id.ToLowerInvariant(), version);
        Directory.CreateDirectory(path);
        File.WriteAllText(Path.Combine(path, id.ToLowerInvariant() + ".nuspec"),
            $"""
            <?xml version="1.0" encoding="utf-8"?>
            <package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd">
              <metadata>
                <id>{id}</id>
                <version>{version}</version>
                <authors>test</authors>
                <description>test</description>
                <developmentDependency>{developmentDependency.ToString().ToLowerInvariant()}</developmentDependency>
              </metadata>
            </package>
            """);

        return new TaskItem(id + "/" + version, new Metadata
        {
            { "ResolvedPath", path },
        });
    }
}
