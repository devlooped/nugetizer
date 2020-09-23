using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace NuGetizer
{
    public class given_a_localized_library
	{
		ITestOutputHelper output;

		public given_a_localized_library(ITestOutputHelper output)
		{
			this.output = output;
		}

		[Fact]
		public void when_getting_package_contents_then_contains_localized_resources()
		{
			var result = Builder.BuildScenario(nameof(given_a_localized_library));

			result.AssertSuccess(output);

			Assert.Contains(result.Items, i => i.GetMetadata("PackagePath") == "lib\\net45\\es-AR\\library.resources.dll");
		}
	}
}
