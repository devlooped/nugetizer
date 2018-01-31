using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace NuGet.Build.Packaging
{
	// NOTE: not added to the main project because it's built using MSBuild 14 and it's not working 
	// by adding binding redirects, appdomain isolation, etc. :(
	public class given_a_packaging_project_with_netstandard
	{
		ITestOutputHelper output;

		public given_a_packaging_project_with_netstandard(ITestOutputHelper output) => this.output = output;
        
		[Fact(Skip = "Cannot be run from MSBuild 14.x")]
		public void can_get_content_from_referenced_single_targeting_netstandard()
		{
			var result = Builder.BuildScenario(nameof(given_a_packaging_project_with_netstandard),
				target: "GetPackageContents", 
				properties: new { SimulateCrossTargeting = "false" },
				//verbosity: Microsoft.Build.Framework.LoggerVerbosity.Diagnostic,
				output: output);

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\netstandard1.6\b.dll",
			}));
		}

		[Fact(Skip = "Cannot be run from MSBuild 14.x")]
		public void can_get_content_from_referenced_cross_targeting_netstandard()
		{
			var result = Builder.BuildScenario(nameof(given_a_packaging_project_with_netstandard),
				target: "GetPackageContents",
				properties: new { SimulateCrossTargeting = "true" },
				//verbosity: Microsoft.Build.Framework.LoggerVerbosity.Diagnostic,
				output: output);

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\net45\b.dll",
			}));
			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\netstandard1.6\b.dll",
			}));
		}

	}
}
