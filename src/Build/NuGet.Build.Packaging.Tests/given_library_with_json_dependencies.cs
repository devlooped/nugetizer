using System.Linq;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Xunit;
using Xunit.Abstractions;

namespace NuGet.Build.Packaging
{
	public class given_library_with_json_dependencies
	{
		ITestOutputHelper output;

		public given_library_with_json_dependencies(ITestOutputHelper output)
		{
			this.output = output;
		}

		[Fact]
		public void when_getting_package_contents_then_can_remove_development_dependency()
		{
			var result = Builder.BuildScenario(nameof(given_library_with_json_dependencies), output: output);

			Assert.Equal(TargetResultCode.Success, result.ResultCode);

			Assert.DoesNotContain(result.Items, item => item.Matches(new
			{
				Kind = PackageItemKind.Dependency,
				Identity = "IFluentInterface",
			}));
		}

		[Fact]
		public void when_getting_package_contents_then_contains_package_reference_to_nuget()
		{
			var result = Builder.BuildScenario(nameof(given_library_with_json_dependencies), output: output);

			Assert.Equal(TargetResultCode.Success, result.ResultCode);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				Kind = PackageItemKind.Dependency,
				Identity = "Newtonsoft.Json",
			}));
			Assert.Contains(result.Items, item => item.Matches(new
			{
				Kind = PackageItemKind.Dependency,
				Identity = "Microsoft.CodeAnalysis",
			}));
		}
	}
}
