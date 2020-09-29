using Microsoft.Build.Execution;
using Xunit;
using Xunit.Abstractions;

namespace NuGetizer
{
    public class given_a_library_with_non_nugetized_reference
	{
		ITestOutputHelper output;

		public given_a_library_with_non_nugetized_reference(ITestOutputHelper output) => this.output = output;
        
		[Fact]
		public void when_getting_contents_then_fails()
		{
			var properties = new
			{
				Configuration = "Release",
				PackFrameworkReferences = "false",
            };

			Builder.BuildScenario(nameof(given_a_library_with_non_nugetized_reference), properties, projectName: "a", target: "Restore")
                .AssertSuccess(output);

			var result = Builder.BuildScenario(nameof(given_a_library_with_non_nugetized_reference), properties, projectName: "a", target: "GetPackageContents");

			Assert.Equal(TargetResultCode.Failure, result.ResultCode);
			Assert.Contains(result.Logger.Errors, error => error.Code == "NG0011");
		}

		[Fact]
		public void when_include_in_package_false_then_does_not_fail()
		{
			var properties = new
			{
				Configuration = "Release",
				PackFrameworkReferences = "false",
				IncludeInPackage = "false",
            };
			
            Builder.BuildScenario(nameof(given_a_library_with_non_nugetized_reference), properties, projectName: "a", target: "Restore")
                .AssertSuccess(output);

			var result = Builder.BuildScenario(nameof(given_a_library_with_non_nugetized_reference), properties, projectName: "a", target: "GetPackageContents", output: output);

			result.AssertSuccess(output);
		}
	}
}
