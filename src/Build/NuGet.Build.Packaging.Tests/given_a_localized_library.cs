using System.Linq;
using Microsoft.Build.Execution;
using Xunit;
using Xunit.Abstractions;

namespace NuGet.Build.Packaging
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

			Assert.True(result.Items.Any(i => i.GetMetadata("PackagePath") == "lib\\net45\\es-AR\\library.resources.dll"));
		}
	}
}
