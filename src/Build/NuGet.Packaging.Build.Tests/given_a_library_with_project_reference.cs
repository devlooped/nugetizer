using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace NuGet.Packaging
{
	public class given_a_library_with_project_reference
	{
		ITestOutputHelper output;

		public given_a_library_with_project_reference(ITestOutputHelper output)
		{
			this.output = output;
		}

		//[Verbosity(LoggerVerbosity.Diagnostic)]
		[Fact]
		public void when_getting_package_contents_then_retrieves_main_assembly_transitively()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_project_reference));

			Assert.Equal(TargetResultCode.Success, result.ResultCode);

			// TODO: build some helpers to make this easier to assert.
			Assert.True(result.Items.Any(i => i.GetMetadata("FileName") == "a" && i.GetMetadata("Extension") == ".dll" && i.GetMetadata("Kind") == "Lib"), "Did not include main project output as Library");
			Assert.True(result.Items.Any(i => i.GetMetadata("FileName") == "b" && i.GetMetadata("Extension") == ".dll" && i.GetMetadata("Kind") == "Lib"), "Did not include referenced project output as Library");
		}

		[Fact]
		public void when_getting_package_contents_then_retrieves_symbols_transitively()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_project_reference), output: output);

			Assert.Equal(TargetResultCode.Success, result.ResultCode);

			// TODO: build some helpers to make this easier to assert.
			Assert.True(result.Items.Any(i => i.GetMetadata("FileName") == "a" && i.GetMetadata("Extension") == ".pdb" && i.GetMetadata("Kind") == "Symbols"), "Did not include main project symbols");
			Assert.True(result.Items.Any(i => i.GetMetadata("FileName") == "b" && i.GetMetadata("Extension") == ".pdb" && i.GetMetadata("Kind") == "Symbols"), "Did not include referenced project symbols");
		}
	}
}
