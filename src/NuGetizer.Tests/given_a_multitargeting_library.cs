using Xunit;
using Xunit.Abstractions;

namespace NuGetizer
{
    public class given_a_multitargeting_library
    {
		ITestOutputHelper output;

		public given_a_multitargeting_library(ITestOutputHelper output)
		{
			this.output = output;
            Builder.BuildScenario(nameof(given_a_multitargeting_library), target: "Restore", output: output)
                .AssertSuccess(output);
        }

        [Fact]
		public void when_gettingcontents_then_includes_content_from_all_frameworks()
		{
            var result = Builder.BuildScenario(nameof(given_a_multitargeting_library), output: output);
			result.AssertSuccess(output);

            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = "lib\\netstandard2.0\\library.dll",
            }));

            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = "lib\\net472\\library.dll",
            }));
        }

        [Fact]
        public void when_gettingcontents_then_includes_single_metadata()
        {
            var result = Builder.BuildScenario(nameof(given_a_multitargeting_library), output: output);
            result.AssertSuccess(output);

            Assert.Single(result.Items, item => item.Matches(new
            {
                PackFolder = PackFolderKind.Metadata,
                TargetFrameworkMoniker = "",
            }));
        }

        [Fact]
        public void when_packing_then_succeeds()
        {
            Builder.BuildScenario(nameof(given_a_multitargeting_library), target: "Pack", output: output)
                .AssertSuccess(output);
        }

        [Fact]
        public void when_customizing_item_definition_then_adds_package_metadata()
        {
            var result = Builder.BuildScenario(nameof(given_a_multitargeting_library), new
            {
                CustomizeItemDefinition = "true"
            });
            result.AssertSuccess(output);

            Assert.Single(result.Items, item => item.Matches(new
            {
                PackFolder = PackFolderKind.Metadata,
                NewMetadata = "Foo",
            }));
        }

        [Fact]
        public void when_customizing_item_then_adds_package_metadata()
        {
            var result = Builder.BuildScenario(nameof(given_a_multitargeting_library), new
            {
                CustomizeItem = "true",
                Description = "Customized"
            });
            result.AssertSuccess(output);

            Assert.Single(result.Items, item => item.Matches(new
            {
                PackFolder = PackFolderKind.Metadata,
                Description = "Customized",
                LicenseExpression = "MIT",
            }));
        }

    }
}
