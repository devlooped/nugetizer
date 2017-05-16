using System.Linq;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Xunit;
using Xunit.Abstractions;

namespace NuGet.Build.Packaging
{
	public class given_a_library_with_config_dependencies
	{
		ITestOutputHelper output;

		public given_a_library_with_config_dependencies(ITestOutputHelper output)
		{
			this.output = output;
		}

		[Fact]
		public void when_getting_package_contents_then_does_not_contain_development_dependency()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_config_dependencies), output: output);

			result.AssertSuccess(output);

			Assert.DoesNotContain(result.Items, item => item.Matches(new
			{
				Kind = PackageItemKind.Dependency,
				Identity = "IFluentInterface",
			}));
		}

		[Fact]
		public void when_getting_package_contents_then_can_remove_arbitrary_dependencies()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_config_dependencies), output: output);

			result.AssertSuccess(output);

			Assert.DoesNotContain(result.Items, item => item.Matches(new
			{
				Kind = PackageItemKind.Dependency,
				Identity = "Microsoft.CodeAnalysis.Analyzers",
			}));
			Assert.DoesNotContain(result.Items, item => item.Matches(new
			{
				Kind = PackageItemKind.Dependency,
				Identity = "Microsoft.CodeAnalysis.Common",
			}));
			Assert.DoesNotContain(result.Items, item => item.Matches(new
			{
				Kind = PackageItemKind.Dependency,
				Identity = "Microsoft.CodeAnalysis.CSharp",
			}));
			Assert.DoesNotContain(result.Items, item => item.Matches(new
			{
				Kind = PackageItemKind.Dependency,
				Identity = "Microsoft.CodeAnalysis.CSharp.Workspaces",
			}));
			Assert.DoesNotContain(result.Items, item => item.Matches(new
			{
				Kind = PackageItemKind.Dependency,
				Identity = "Microsoft.CodeAnalysis.VisualBasic",
			}));
			Assert.DoesNotContain(result.Items, item => item.Matches(new
			{
				Kind = PackageItemKind.Dependency,
				Identity = "Microsoft.CodeAnalysis.VisualBasic.Workspaces",
			}));
			Assert.DoesNotContain(result.Items, item => item.Matches(new
			{
				Kind = PackageItemKind.Dependency,
				Identity = "Microsoft.CodeAnalysis.Workspaces.Common",
			}));
		}

		[Fact]
		public void when_getting_package_contents_then_contains_package_reference_to_nuget()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_config_dependencies), output: output);

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				Kind = PackageItemKind.Dependency,
				Identity = "Newtonsoft.Json",
			}));
		}

		[Fact]
		public void when_not_inferring_legacy_package_references_then_does_not_contain_package_reference_to_nuget()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_config_dependencies), 
				properties: new { InferLegacyPackageReferences = false }, 
				output: output);

			result.AssertSuccess(output);

			Assert.DoesNotContain(result.Items, item => item.Matches(new
			{
				Kind = PackageItemKind.Dependency,
				Identity = "Newtonsoft.Json",
			}));
		}
	}
}
