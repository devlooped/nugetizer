using System.Linq;
using Microsoft.Build.Execution;
using Xunit;
using Xunit.Abstractions;

namespace NuGet.Packaging
{
	public class given_an_empty_library_project
	{
		ITestOutputHelper output;

		public given_an_empty_library_project(ITestOutputHelper output)
		{
			this.output = output;
		}

		[Fact]
		public void when_getting_package_contents_then_retrieves_main_assembly()
		{
			var result = Builder.BuildScenario(nameof(given_an_empty_library_project));

			Assert.Equal(TargetResultCode.Success, result.ResultCode);

			Assert.True(result.Items.Any(i => i.GetMetadata("Extension") == ".dll" && i.GetMetadata("Kind") == "Lib"), "Did not include main output");
		}

		[Fact]
		public void when_getting_package_contents_then_retrieves_symbols()
		{
			var result = Builder.BuildScenario(nameof(given_an_empty_library_project), new { Configuration = "Debug" });

			Assert.Equal(TargetResultCode.Success, result.ResultCode);

			Assert.True(result.Items.Any(i => i.GetMetadata("Extension") == ".pdb" && i.GetMetadata("Kind") == "Symbols"), "Did not include main symbols");
		}

		[Fact]
		public void when_getting_package_contents_then_annotates_items_with_package_id()
		{
			var result = Builder.BuildScenario(nameof(given_an_empty_library_project), new { PackageId = "Foo" });

			Assert.Equal(TargetResultCode.Success, result.ResultCode);

			Assert.True(result.Items.All(i => i.GetMetadata("PackageId") == "Foo"), "Did not annotate contents with package id.");
		}
	}
}
