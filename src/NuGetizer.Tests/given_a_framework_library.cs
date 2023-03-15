using Xunit;
using Xunit.Abstractions;

namespace NuGetizer
{
    public class given_a_framework_library
    {
        readonly ITestOutputHelper output;

        public given_a_framework_library(ITestOutputHelper output)
        {
            this.output = output;
            Builder.BuildScenario(nameof(given_a_framework_library), target: "Restore")
                .AssertSuccess(output);
        }

        [Fact]
        public void when_getting_package_contents_then_includes_framework_references_by_default()
        {
            var result = Builder.BuildScenario(nameof(given_a_framework_library));

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                Identity = "PresentationCore",
                PackFolder = PackFolderKind.FrameworkReference,
            }));
        }

        [Fact]
        public void when_include_outputs_in_package_is_false_then_does_not_include_main_assembly()
        {
            var result = Builder.BuildScenario(nameof(given_a_framework_library), new
            {
                IncludeWPF = false,
            });

            result.AssertSuccess(output);
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                Identity = "PresentationCore",
                PackFolder = PackFolderKind.FrameworkReference,
            }));
        }
    }
}
