using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Xunit;
using Xunit.Abstractions;
using NuGet.Build.Packaging.Tasks;
using NuGet.Versioning;

namespace NuGet.Build.Packaging
{
	public class ReadLegacyJsonDependenciesTests
	{
		ITestOutputHelper output;
		MockBuildEngine engine;

		public ReadLegacyJsonDependenciesTests(ITestOutputHelper output)
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
				File.WriteAllText(configPath, @"{
  ""dependencies"": {
    ""Newtonsoft.Json"": ""9.0.1""
  },
  ""frameworks"": {
    ""net45"": { }
  },
  ""runtimes"": {
    ""win"": { }
  }
}");

				var task = new ReadLegacyJsonDependencies
				{
					BuildEngine = engine,
					ProjectJsonPath = new TaskItem(configPath)
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
	}
}
