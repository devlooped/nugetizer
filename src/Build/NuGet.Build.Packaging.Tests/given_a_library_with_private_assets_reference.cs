using System.Linq;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Xunit;
using Xunit.Abstractions;

namespace NuGet.Build.Packaging
{
	public class given_a_library_with_private_assets_reference
	{
		ITestOutputHelper output;

		public given_a_library_with_private_assets_reference(ITestOutputHelper output)
		{
			this.output = output;
		}

		[Fact]
		public void when_getting_package_contents_then_contains_private_assets_as_primary_output()
		{
			Builder.BuildScenario(nameof(given_a_library_with_private_assets_reference), target: "Restore", output: output);
			var result = Builder.BuildScenario(nameof(given_a_library_with_private_assets_reference), output: output);

			result.AssertSuccess(output);

			Assert.DoesNotContain(result.Items, item => item.Matches(new
			{
				Kind = PackageItemKind.Dependency,
				Identity = "Mono.Options",
			}));
			Assert.Contains(result.Items, item => item.Matches(new
			{
				Kind = PackageItemKind.Lib,
				Filename = "Mono.Options",
				NuGetSourceType = "Package",
				NuGetPackageId = "Mono.Options",
			}));
		}

		[Fact]
		public void when_getting_package_contents_then_contains_private_lib_assets_as_primary_output_and_also_package_reference()
		{
			Builder.BuildScenario(nameof(given_a_library_with_private_assets_reference), target: "Restore", output: output);
			var result = Builder.BuildScenario(nameof(given_a_library_with_private_assets_reference), output: output);

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				Kind = PackageItemKind.Dependency,
				Identity = "Newtonsoft.Json",
			}));
			Assert.Contains(result.Items, item => item.Matches(new
			{
				Kind = PackageItemKind.Lib,
				Filename = "Newtonsoft.Json",
				NuGetSourceType = "Package",
				NuGetPackageId = "Newtonsoft.Json",
			}));
		}

		[Fact]
		public void when_getting_package_contents_then_contains_dependency_for_non_private_assets_reference()
		{
			Builder.BuildScenario(nameof(given_a_library_with_private_assets_reference), target: "Restore", output: output);
			var result = Builder.BuildScenario(nameof(given_a_library_with_private_assets_reference), output: output);

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				Kind = PackageItemKind.Dependency,
				Identity = "xunit",
			}));
			Assert.DoesNotContain(result.Items, item => item.Matches(new
			{
				Kind = PackageItemKind.Lib,
				Filename = "xunit",
				NuGetSourceType = "Package",
				NuGetPackageId = "xunit",
			}));
		}
	}
}
