using System.Linq;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Xunit;
using Xunit.Abstractions;

namespace NuGet.Packaging
{
	public class given_library_project_with_nugets
	{
		ITestOutputHelper output;

		public given_library_project_with_nugets(ITestOutputHelper output)
		{
			this.output = output;
		}

		[Fact(Skip = "Pending adding actual project.json/packages.config reading. project.lock.json-based dependencies don't give us what we need.")]
		public void when_getting_package_contents_then_contains_package_reference_to_nuget()
		{
			var result = Builder.BuildScenario(nameof(given_library_project_with_nugets), output: output);

			Assert.Equal(TargetResultCode.Success, result.ResultCode);

			var package = result.Items.FirstOrDefault(i => i.GetMetadata("Kind") == "Dependency");

			Assert.NotNull(package);
			Assert.Equal(package.ItemSpec, "Newtonsoft.Json");
			Assert.Equal(package.GetMetadata("Version"), "8.0.3");
		}
	}
}
