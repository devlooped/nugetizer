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
	public class given_a_packaging_project
	{
		ITestOutputHelper output;

		public given_a_packaging_project(ITestOutputHelper output)
		{
			this.output = output;
		}
        
		[Fact]
		public void when_getting_contents_then_includes_referenced_project_outputs()
		{
			var result = Builder.BuildScenario(nameof(given_a_packaging_project), target: "GetPackageContents", output: output);

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\net46\b.dll",
			}));
			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\net46\b.xml",
			}));
			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\net46\b.dll",
			}));
		}

		[Fact]
		public void when_getting_contents_then_includes_referenced_project_satellite_assembly()
		{
			var result = Builder.BuildScenario(nameof(given_a_packaging_project), target: "GetPackageContents", output: output);

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\net46\es-AR\b.resources.dll",
			}));
		}

		[Fact]
		public void when_getting_contents_then_includes_referenced_project_dependencies()
		{
			var result = Builder.BuildScenario(nameof(given_a_packaging_project), target: "GetPackageContents", output: output);

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\net46\d.dll",
			}));
			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\net46\d.dll",
			}));
		}

		[Fact]
		public void when_getting_contents_then_includes_referenced_project_dependency_satellite_assembly()
		{
			var result = Builder.BuildScenario(nameof(given_a_packaging_project), target: "GetPackageContents", output: output);

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\net46\es-AR\d.resources.dll",
			}));
		}

		[Fact]
		public void when_getting_contents_then_includes_referenced_packagable_project_as_dependency()
		{
			var result = Builder.BuildScenario(nameof(given_a_packaging_project), target: "GetPackageContents", output: output);

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				Kind = "Dependency",
				Identity = "E",
			}));
		}

		[Fact]
		public void when_getting_contents_then_does_not_include_referenced_project_nuget_assembly_reference()
		{
			var result = Builder.BuildScenario(nameof(given_a_packaging_project), target: "GetPackageContents", output: output);

			result.AssertSuccess(output);

			Assert.DoesNotContain(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\net46\Newtonsoft.Json.dll",
			}));
		}

		[Fact]
		public void when_getting_contents_from_packaging_project_then_referenced_outputs_have_original_tfm_path()
		{
			var result = Builder.BuildScenario(nameof(given_a_packaging_project), target: "GetPackageContents", output: output);

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\net45\c.dll",
			}));
			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\net45\c.xml",
			}));
		}
	}
}
