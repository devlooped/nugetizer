using Xunit;
using Xunit.Abstractions;

namespace NuGetizer
{
    public class given_a_multi_platform_solution
    {
        ITestOutputHelper output;

        public given_a_multi_platform_solution(ITestOutputHelper output)
        {
            this.output = output;
            Builder.BuildScenario(nameof(given_a_multi_platform_solution), target: "Restore", output: output)
                .AssertSuccess(output);
        }

        [Fact]
        public void then_includes_primary_output_from_platforms()
        {
            var result = Builder.BuildScenario(nameof(given_a_multi_platform_solution), output: output);

            result.AssertSuccess(output);

            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/net472/Forms.dll"
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/xamarinios10/Forms.dll"
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/monoandroid90/Forms.dll"
            }));
        }

        [Fact]
        public void then_includes_direct_dependency_from_platforms()
        {
            var result = Builder.BuildScenario(nameof(given_a_multi_platform_solution), output: output);

            result.AssertSuccess(output);

            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/net472/Common.dll"
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/xamarinios10/Common.dll"
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/monoandroid90/Common.dll"
            }));
        }

        [Fact]
        public void then_includes_platform_and_language_quickstart_content()
        {
            var result = Builder.BuildScenario(nameof(given_a_multi_platform_solution), output: output);

            result.AssertSuccess(output);

            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"contentFiles/cs/monoandroid90/quickstart/sample.cs"
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"contentFiles/vb/monoandroid90/quickstart/sample.vb"
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"contentFiles/fs/monoandroid90/quickstart/sample.fs"
            }));
        }

        [Fact]
        public void then_includes_platform_docs_from_before_get_package_contents()
        {
            var result = Builder.BuildScenario(nameof(given_a_multi_platform_solution), output: output);

            result.AssertSuccess(output);

            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"docs/gettingstarted.html"
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"docs/overview/index.html"
            }));
        }

    }
}
