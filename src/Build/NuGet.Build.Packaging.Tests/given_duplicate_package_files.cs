using System.Linq;
using Microsoft.Build.Execution;
using Xunit;
using Xunit.Abstractions;

namespace NuGet.Build.Packaging
{
	public class given_duplicate_package_files
	{
		ITestOutputHelper output;

		public given_duplicate_package_files(ITestOutputHelper output)
		{
			this.output = output;
		}

		[Fact]
		public void build_fails()
		{
			var result = Builder.BuildScenario(nameof(given_duplicate_package_files), target: "Pack");

			Assert.Equal(TargetResultCode.Failure, result.ResultCode);

			// content.txt is marked as PackageFile and the path to the file is exacly the same in both project a and b.
			Assert.False(result.Logger.Errors.Any(e => e.Message.Contains("content.txt")));

			// nuget.js is marked as content and the path to the file is "bin\{projectname}\content\web\js\nuget.js" so to the package they are two conflicting files.
			Assert.True(result.Logger.Errors.Any(e => e.Message.Contains("nuget.js")));
		}
	}
}
