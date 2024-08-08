using Xunit;
using Xunit.Abstractions;

namespace NuGetizer
{
    public class given_transitive_projects
    {
        readonly ITestOutputHelper output;

        public given_transitive_projects(ITestOutputHelper output)
        {
            this.output = output;
            using var disable = OpenBuildLogAttribute.Disable();

            Builder.BuildScenario(nameof(given_transitive_projects), target: "Restore", projectName: "Project1")
                .AssertSuccess(output);

            Builder.BuildScenario(nameof(given_transitive_projects), target: "Build", projectName: "Project1")
                .AssertSuccess(output);
        }

        [Fact]
        public void when_pack_no_build_then_succeeds()
        {
            var result = Builder.BuildScenario(nameof(given_transitive_projects),
                projectName: "Project1",
                // These are the properties passed by dotnet pack --no-build
                properties: new { NoBuild = "true", _IsPacking = "true" },
                target: "GetPackageContents,Pack");

            result.AssertSuccess(output);

            Assert.True(result.BuildResult.HasResultsForTarget("GetPackageContents"));

            var items = result.BuildResult.ResultsByTarget["GetPackageContents"];

            Assert.Contains(items.Items, item => item.Matches(new
            {
                PackagePath = "lib/net8.0/Project1.dll"
            }));

            Assert.Contains(items.Items, item => item.Matches(new
            {
                PackagePath = "lib/net8.0/Project2.dll"
            }));

            Assert.Contains(items.Items, item => item.Matches(new
            {
                PackagePath = "lib/net8.0/Project3.dll"
            }));
        }
    }
}
