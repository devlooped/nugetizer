using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using NuGet.Build.Packaging.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace NuGet.Build.Packaging
{
	public class given_a_library_with_non_nugetized_reference
	{
		ITestOutputHelper output;

		public given_a_library_with_non_nugetized_reference(ITestOutputHelper output)
		{
			this.output = output;
		}

		[Fact]
		public void when_getting_nugetized_target_path_then_it_contains_ispackable_metadata()
		{
			var result = Builder.BuildScenario(
				nameof(given_a_library_with_non_nugetized_reference), 
				target: "GetTargetPath",
				output: output);

			Assert.Equal(TargetResultCode.Success, result.ResultCode);

			Assert.True(result.Items[0].MetadataNames.OfType<string>().Contains(MetadataName.IsPackable));
		}

		[Fact]
		public void when_getting_non_nugetized_target_path_then_it_does_not_contains_ispackable_metadata()
		{
			var result = Builder.BuildScenario(
				nameof(given_a_library_with_non_nugetized_reference),
				projectName: "b",
				target: "GetTargetPath",
				output: output);

			Assert.Equal(TargetResultCode.Success, result.ResultCode);

			Assert.False(result.Items[0].MetadataNames.OfType<string>().Contains(MetadataName.IsPackable));
		}

		[Fact]
		public void when_getting_package_contents_then_fails_for_non_nugetized_reference()
		{
			var result = Builder.BuildScenario(
				nameof(given_a_library_with_non_nugetized_reference),
				target: "GetPackageContents",
				output: output);

			Assert.Equal(TargetResultCode.Failure, result.ResultCode);
		}
	}
}
