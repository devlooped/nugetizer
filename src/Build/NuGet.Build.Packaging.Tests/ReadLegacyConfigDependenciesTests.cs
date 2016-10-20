using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Xunit;
using Xunit.Abstractions;
using NuGet.Build.Packaging.Tasks;
using NuGet.Versioning;

namespace NuGet.Build.Packaging
{
	public class ReadLegacyConfigDependenciesTests
	{
		ITestOutputHelper output;
		MockBuildEngine engine;

		public ReadLegacyConfigDependenciesTests(ITestOutputHelper output)
		{
			this.output = output;
			engine = new MockBuildEngine(output);
		}

		[Fact]
		public void when_config_has_dependency_then_projects_package_reference()
		{
			var configPath = Path.GetTempFileName();
			try
			{
				File.WriteAllText(configPath, @"<packages>
  <package id='Newtonsoft.Json' version='9.0.1' targetFramework='net45' />
</packages>");

				var task = new ReadLegacyConfigDependencies
				{
					BuildEngine = engine,
					PackagesConfigPath = new TaskItem(configPath)
				};

				Assert.True(task.Execute());

				Assert.Contains(task.PackageReferences, item => item.Matches(new
				{
					Identity = "Newtonsoft.Json",
					Version = VersionRange.Parse("9.0.1").ToString()
				}));
			}
			finally
			{
				File.Delete(configPath);
			}
		}

		[Fact]
		public void when_config_has_development_dependency_then_skips_package_reference()
		{
			var configPath = Path.GetTempFileName();
			try
			{
				File.WriteAllText(configPath, @"<packages>
  <package id='IFluentInterface' version='2.0.3' targetFramework='net45' developmentDependency='true' />
</packages>");

				var task = new ReadLegacyConfigDependencies
				{
					BuildEngine = engine,
					PackagesConfigPath = new TaskItem(configPath)
				};

				Assert.True(task.Execute());
				Assert.Equal(0, task.PackageReferences.Length);
			}
			finally
			{
				File.Delete(configPath);
			}
		}
	}
}
