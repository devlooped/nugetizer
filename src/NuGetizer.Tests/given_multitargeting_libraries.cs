using System.IO;
using NuGetizer.Tests;
using Xunit;
using Xunit.Abstractions;

namespace NuGetizer
{
    public class given_multitargeting_libraries
    {
        readonly ITestOutputHelper output;

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

        [RuntimeFact("Windows")]
        public void when_getting_content_then_multitargets()
        {
            Builder.BuildScenario(
                nameof(given_multitargeting_libraries),
                projectName: "uilibrary.csproj", target: "Restore", output: output)
                .AssertSuccess(output);

            var result = Builder.BuildScenario(
                nameof(given_multitargeting_libraries),
                projectName: "uilibrary.csproj",
                target: "GetPackageContents", output: output);

            result.AssertSuccess(output);

            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = "lib/net9.0/uilibrary.dll"
            }));

            Assert.Contains(result.Items, item => item.TryGetMetadata("PackagePath", out var path) &&
                path.StartsWith("lib/net9.0-windows") && path.EndsWith("uilibrary.dll"));

            Assert.Contains(result.Items, item => item.TryGetMetadata("PackagePath", out var path) &&
                path.StartsWith("lib/net9.0-maccatalyst") && path.EndsWith("uilibrary.dll"));
        }
    }
}
