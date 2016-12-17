using System;
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
		public void when_getting_contents_then_fails()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_non_nugetized_reference), new
			{
				Configuration = "Release",
				IncludeFrameworkReferences = "false",
			}, projectName: "a", target: "GetPackageContents", output: output);

			Assert.Equal(TargetResultCode.Failure, result.ResultCode);
			Assert.Contains(result.Logger.Errors, error => error.Code == "NG0011");
		}

		[Fact]
		public void when_include_in_package_false_then_does_not_fail()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_non_nugetized_reference), properties: new
			{
				Configuration = "Release",
				IncludeFrameworkReferences = "false",
				IncludeInPackage = "false"
			}, projectName: "a", target: "GetPackageContents", output: output);

			Assert.Equal(TargetResultCode.Success, result.ResultCode);
		}
	}
}
