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

			Assert.True(result.Logger.Errors.Any(e => e.Message.Contains("content.txt")));
			Assert.True(result.Logger.Errors.Any(e => e.Message.Contains("nuget.js")));
		}
	}
}
