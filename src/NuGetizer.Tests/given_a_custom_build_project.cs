using Xunit;
using Xunit.Abstractions;

namespace NuGetizer
{
    public class given_a_custom_build_project
    {
        readonly ITestOutputHelper output;

        public given_a_custom_build_project(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void when_packing_then_can_include_content()
        {
            var result = Builder.BuildScenario(nameof(given_a_custom_build_project), output: output);

            result.AssertSuccess(output);

            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = "Readme.txt",
            }));
        }
    }
}
