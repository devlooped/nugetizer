using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace NuGetizer
{
    public class given_a_library
    {
        ITestOutputHelper output;

        public given_a_library(ITestOutputHelper output)
        {
            this.output = output;
            using var disable = OpenBuildLogAttribute.Disable();
            Builder.BuildScenario(nameof(given_a_library), target: "Restore")
                .AssertSuccess(output);
        }

        [Fact]
        public void when_pack_compile_then_excludes_generated_files()
        {
            var result = Builder.BuildScenario(nameof(given_a_library),
                new { PackCompile = "true" },
                target: "Build,GetPackageContents,Pack");

            Assert.True(result.BuildResult.HasResultsForTarget("GetPackageContents"));

            var items = result.BuildResult.ResultsByTarget["GetPackageContents"];
            var compile = items.Items.Where(item => item.Matches(new
            {
                BuildAction = "Compile",
            })).ToArray();

            Assert.Equal(2, compile.Length);
        }

        [Fact]
        public void when_pack_excludes_additional_items_then_contains_only_matching_files()
        {
            var result = Builder.BuildScenario(nameof(given_a_library),
                new { PackCompile = "true", PackOnlyApi = "true" },
                target: "Build,GetPackageContents,Pack");

            Assert.True(result.BuildResult.HasResultsForTarget("GetPackageContents"));

            var items = result.BuildResult.ResultsByTarget["GetPackageContents"];
            var compile = items.Items.Where(item => item.Matches(new
            {
                BuildAction = "Compile",
            })).ToArray();

            Assert.Single(compile);
        }
    }
}
