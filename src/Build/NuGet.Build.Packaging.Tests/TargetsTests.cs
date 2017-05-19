using System.Collections.Generic;
using Microsoft.Build.Evaluation;
using Xunit;

namespace NuGet.Build.Packaging
{
	public class TargetsTests
	{
		[Fact]
		public void IncludeFrameworkReferencesInPackage_defaults_to_false_for_build_primary_output()
		{
			var project = new Project("NuGet.Build.Packaging.targets", new Dictionary<string, string>
			{
				{ "PrimaryOutputPackageFileKind", "build" }
			}, "14.0");

			Assert.Equal("false", project.GetPropertyValue("IncludeFrameworkReferencesInPackage"));
		}

		[Fact]
		public void IncludeFrameworkReferencesInPackage_defaults_to_false_for_tool_primary_output()
		{
			var project = new Project("NuGet.Build.Packaging.targets", new Dictionary<string, string>
			{
				{ "PrimaryOutputPackageFileKind", "tool" }
			}, "14.0");

			Assert.Equal("false", project.GetPropertyValue("IncludeFrameworkReferencesInPackage"));
		}

		[Fact]
		public void IncludeFrameworkReferencesInPackage_defaults_to_false_for_tools_primary_output()
		{
			var project = new Project("NuGet.Build.Packaging.targets", new Dictionary<string, string>
			{
				{ "PrimaryOutputPackageFileKind", "tools" }
			}, "14.0");

			Assert.Equal("false", project.GetPropertyValue("IncludeFrameworkReferencesInPackage"));
		}

		[Fact]
		public void IncludeFrameworkReferencesInPackage_defaults_to_true_for_default_primary_output()
		{
			var project = new Project("NuGet.Build.Packaging.targets", new Dictionary<string, string>
			{
			}, "14.0");

			Assert.Equal("true", project.GetPropertyValue("IncludeFrameworkReferencesInPackage"));
		}

		[Fact]
		public void IncludeFrameworkReferencesInPackage_defaults_to_false_for_compat_istool()
		{
			var project = new Project("NuGet.Build.Packaging.targets", new Dictionary<string, string>
			{
				{ "IsTool", "true" }
			}, "14.0");

			Assert.Equal("false", project.GetPropertyValue("IncludeFrameworkReferencesInPackage"));
		}

		[Fact]
		public void PackOnBuild_defaults_to_true_for_compat_GeneratePackageOnBuild_true()
		{
			var project = new Project("NuGet.Build.Packaging.targets", new Dictionary<string, string>
			{
				{ "GeneratePackageOnBuild", "true" }
			}, "14.0");

			Assert.Equal("true", project.GetPropertyValue("PackOnBuild"));
		}

		
	}
}
