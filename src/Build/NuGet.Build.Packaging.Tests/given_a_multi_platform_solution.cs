using System.Linq;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using NuGet.Build.Packaging.Tasks;
using NuGet.Frameworks;
using NuGet.Packaging;
using Xunit;
using Xunit.Abstractions;

namespace NuGet.Build.Packaging
{
	public class given_a_multi_platform_solution
	{
		ITestOutputHelper output;

		public given_a_multi_platform_solution(ITestOutputHelper output)
		{
			this.output = output;
		}

		[Fact]
		public void then_includes_primary_output_from_platforms()
		{
			var result = Builder.BuildScenario(nameof(given_a_multi_platform_solution), output: output);

			Assert.Equal(TargetResultCode.Success, result.ResultCode);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\net46\Forms.dll"
			}));
			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\ios10\Forms.dll"
			}));
			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\android70\Forms.dll"
			}));
		}

		[Fact]
		public void then_includes_direct_dependency_from_platforms()
		{
			var result = Builder.BuildScenario(nameof(given_a_multi_platform_solution), output: output);

			Assert.Equal(TargetResultCode.Success, result.ResultCode);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\net46\Common.dll"
			}));
			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\ios10\Common.dll"
			}));
			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\android70\Common.dll"
			}));
		}

		[Fact]
		public void then_includes_platform_and_language_quickstart_content()
		{
			var result = Builder.BuildScenario(nameof(given_a_multi_platform_solution), output: output);

			Assert.Equal(TargetResultCode.Success, result.ResultCode);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"contentFiles\cs\android70\quickstart\sample.cs"
			}));
			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"contentFiles\vb\android70\quickstart\sample.vb"
			}));
			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"contentFiles\fs\android70\quickstart\sample.fs"
			}));
		}

		[Fact]
		public void when_packing_then_includes_same_content_as_items()
		{
			var result = Builder.BuildScenario(nameof(given_a_multi_platform_solution), target: "Pack", output: output);

			Assert.Equal(TargetResultCode.Success, result.ResultCode);

			var manifest = result.Items[0].GetManifest();
		}

		/*
  Forms
        Kind=Metadata
        PackageId=Forms
        PackagePath=
        TargetPath=
        TargetFramework=
        TargetFrameworkMoniker=
  Forms.dll
        Kind=Lib
        PackageId=Forms
        PackagePath=lib\portable50\Forms.dll
        TargetPath=Forms.dll
        TargetFramework=portable50
        TargetFrameworkMoniker=.NETPortable,Version=v5.0
  Forms.dll
        Kind=Lib
        PackageId=Forms
        PackagePath=lib\net46\Forms.dll
        TargetPath=Forms.dll
        TargetFramework=net46
        TargetFrameworkMoniker=.NETFramework,Version=v4.6
  System.Core
        Kind=FrameworkReference
        PackageId=Forms
        PackagePath=
        TargetPath=
        TargetFramework=net46
        TargetFrameworkMoniker=.NETFramework,Version=v4.6
  Common.dll
        Kind=Lib
        PackageId=Forms
        PackagePath=lib\net46\Common.dll
        TargetPath=Common.dll
        TargetFramework=net46
        TargetFrameworkMoniker=.NETFramework,Version=v4.6
  Forms.dll
        Kind=Lib
        PackageId=Forms
        PackagePath=lib\ios10\Forms.dll
        TargetPath=Forms.dll
        TargetFramework=ios10
        TargetFrameworkMoniker=iOS,Version=v1.0
  Common.dll
        Kind=Lib
        PackageId=Forms
        PackagePath=lib\ios10\Common.dll
        TargetPath=Common.dll
        TargetFramework=ios10
        TargetFrameworkMoniker=iOS,Version=v1.0
  Forms.dll
        Kind=Lib
        PackageId=Forms
        PackagePath=lib\android70\Forms.dll
        TargetPath=Forms.dll
        TargetFramework=android70
        TargetFrameworkMoniker=Android,Version=v7.0
  Common.dll
        Kind=Lib
        PackageId=Forms
        PackagePath=lib\android70\Common.dll
        TargetPath=Common.dll
        TargetFramework=android70
        TargetFrameworkMoniker=Android,Version=v7.0*/
	}
}
