using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Xunit;
using Xunit.Abstractions;

namespace NuGetizer
{
    public class TargetsTests
	{
		ITestOutputHelper output;

		public TargetsTests(ITestOutputHelper output) => this.output = output;

		[Fact]
		public void PackFrameworkReferences_is_not_true_for_build_primary_output()
		{
			var project = new Project("NuGetizer.targets", new Dictionary<string, string>
			{
				{ "BuildOutputKind", "build" }
			}, null);

			Assert.NotEqual("true", project.GetPropertyValue("PackFrameworkReferences"));
		}

		[Fact]
		public void PackFrameworkReferences_is_not_true_for_tool_primary_output()
		{
			var project = new Project("NuGetizer.targets", new Dictionary<string, string>
			{
				{ "BuildOutputKind", "tool" }
			}, null);

			Assert.NotEqual("true", project.GetPropertyValue("PackFrameworkReferences"));
		}

		[Fact]
		public void PackFrameworkReferences_is_not_true_for_tools_primary_output()
		{
			var project = new Project("NuGetizer.targets", new Dictionary<string, string>
			{
				{ "BuildOutputKind", "tools" }
			}, null);

			Assert.NotEqual("true", project.GetPropertyValue("PackFrameworkReferences"));
		}

		[Fact]
		public void PackFrameworkReferences_is_true_for_primary_output_lib()
		{
			var project = new Project("NuGetizer.targets", new Dictionary<string, string>
			{
				{ "BuildOutputKind", "lib" }
			}, null);

			Assert.Equal("true", project.GetPropertyValue("PackFrameworkReferences"));
		}

		[Fact]
		public void PackFrameworkReferences_is_true_for_default_primary_output_kind()
		{
			var project = new Project("NuGetizer.targets", new Dictionary<string, string>
			{
			}, null);

			Assert.Equal("true", project.GetPropertyValue("PackFrameworkReferences"));
		}

		[Fact]
		public void PackFrameworkReferences_is_not_true_for_compat_istool()
		{
			var project = new Project("NuGetizer.targets", new Dictionary<string, string>
			{
				{ "IsTool", "true" }
			}, null);

			Assert.NotEqual("true", project.GetPropertyValue("PackFrameworkReferences"));
		}

		[Fact]
		public void PackOnBuild_defaults_to_true_for_compat_GeneratePackageOnBuild_true()
		{
			var project = new Project("NuGetizer.targets", new Dictionary<string, string>
			{
				{ "GeneratePackageOnBuild", "true" }
			}, null);

			Assert.Equal("true", project.GetPropertyValue("PackOnBuild"));
		}

		[Fact]
		public void package_contents_never_includes_nugetizer_package_reference()
		{
			var xml = ProjectRootElement.Create(Path.Combine(Directory.GetCurrentDirectory(), MethodBase.GetCurrentMethod().Name));
			xml.AddImport(@"Scenarios\Scenario.props");
			xml.AddImport(@"Scenarios\Scenario.targets");
			xml.AddItemGroup()
				.AddItem("PackageReference", "NuGetizer", new[] { new KeyValuePair<string, string>("Version", "*") });

			xml.Save();

			var project = new ProjectInstance(xml);

			var result = Builder.Build(project, "GetPackageContents");

			Assert.Equal(BuildResultCode.Success, result.OverallResult);
			Assert.Equal(TargetResultCode.Success, result["GetPackageContents"].ResultCode);

			var items = result["GetPackageContents"].Items;

			Assert.DoesNotContain(items, item => item.Matches(new
			{
				Kind = PackageItemKind.Dependency,
				Identity = "NuGetizer",
			}));
		}
	}
}
