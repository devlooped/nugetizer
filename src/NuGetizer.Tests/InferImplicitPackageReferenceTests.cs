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
}
