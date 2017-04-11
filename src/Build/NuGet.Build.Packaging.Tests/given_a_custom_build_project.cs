using System.Linq;
using Microsoft.Build.Execution;
using Xunit;
using Xunit.Abstractions;

namespace NuGet.Build.Packaging
{
	public class given_a_custom_build_project
	{
		ITestOutputHelper output;

		public given_a_custom_build_project(ITestOutputHelper output)
		{
			this.output = output;
		}

		[Fact]
		public void when_packing_then_can_include_content()
		{
			var result = Builder.BuildScenario(nameof(given_a_custom_build_project));

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = "Readme.txt",
			}));
		}
	}
}
