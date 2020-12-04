using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace NuGetizer
{
    public class given_multitargeting_libraries
    {
        ITestOutputHelper output;

        public given_multitargeting_libraries(ITestOutputHelper output) => this.output = output;

        [InlineData("common.csproj")]
        [InlineData("uilibrary.csproj")]
        [InlineData("uishared.csproj")]
        [Theory]
        public void when_packing_on_build_then_succeeds(string projectName)
        {
            Builder.BuildScenario(
                nameof(given_multitargeting_libraries),
                projectName: projectName, target: "Restore", output: output)
                .AssertSuccess(output);

            Builder.BuildScenario(
                nameof(given_multitargeting_libraries),
                projectName: projectName, target: "Build", output: output)
                .AssertSuccess(output);

            var result = Builder.BuildScenario(
                nameof(given_multitargeting_libraries),
                projectName: projectName,
                target: "GetPackageTargetPath", output: output);

            result.AssertSuccess(output);

            Assert.Single(result.Items);

            Assert.True(File.Exists(result.Items[0].GetMetadata("FullPath")));
        }
    }
}
