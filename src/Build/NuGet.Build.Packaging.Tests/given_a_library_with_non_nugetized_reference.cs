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
		public void when_getting_contents_then_issues_warning_for_missing_nuget()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_non_nugetized_reference), new
			{
				Configuration = "Release",
				IncludeFrameworkReferences = "false",
			}, projectName: "a", target: "GetPackageContents", output: output);

			Assert.Equal(TargetResultCode.Success, result.ResultCode);
			Assert.Contains(result.Logger.Warnings, warning => warning.Code == "NG1001");
		}

		[Fact]
		public void when_include_in_package_false_then_does_not_include_referenced_project_outputs()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_project_reference),
				properties: new { IncludeInPackage = "false" },
				output: output);

			Assert.Equal(TargetResultCode.Success, result.ResultCode);

			Assert.DoesNotContain(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\net46\b.dll",
			}));
			Assert.DoesNotContain(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\net46\b.xml",
			}));
			Assert.DoesNotContain(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\net46\b.dll",
			}));
		}

		[Fact]
		public void when_getting_contents_then_includes_referenced_project_outputs()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_non_nugetized_reference), new
			{
				Configuration = "Release",
				IncludeFrameworkReferences = "false",
			}, projectName: "a", target: "GetPackageContents", output: output);

			Assert.Equal(TargetResultCode.Success, result.ResultCode);

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
			var result = Builder.BuildScenario(nameof(given_a_library_with_non_nugetized_reference), new
			{
				Configuration = "Release",
				IncludeFrameworkReferences = "false",
			}, projectName: "a", target: "GetPackageContents", output: output);

			Assert.Equal(TargetResultCode.Success, result.ResultCode);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\net46\es-AR\b.resources.dll",
			}));
		}

		[Fact]
		public void when_getting_contents_then_includes_referenced_project_dependencies()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_non_nugetized_reference), new
			{
				Configuration = "Release",
				IncludeFrameworkReferences = "false",
			}, projectName: "a", target: "GetPackageContents", output: output);

			Assert.Equal(TargetResultCode.Success, result.ResultCode);

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
		public void when_getting_contents_then_includes_referenced_project__dependency_satellite_assembly()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_non_nugetized_reference), new
			{
				Configuration = "Release",
				IncludeFrameworkReferences = "false",
			}, projectName: "a", target: "GetPackageContents", output: output);

			Assert.Equal(TargetResultCode.Success, result.ResultCode);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\net46\es-AR\d.resources.dll",
			}));
		}

		[Fact]
		public void when_getting_contents_then_does_not_include_referenced_project_nuget_assembly_reference()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_non_nugetized_reference), new
			{
				Configuration = "Release",
				IncludeFrameworkReferences = "false",
			}, projectName: "a", target: "GetPackageContents", output: output);

			Assert.Equal(TargetResultCode.Success, result.ResultCode);

			Assert.DoesNotContain(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\net46\Newtonsoft.Json.dll",
			}));
		}
	}
}
