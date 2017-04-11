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
	public class given_a_packaging_project_with_reference_assembly
	{
		ITestOutputHelper output;

		public given_a_packaging_project_with_reference_assembly(ITestOutputHelper output)
		{
			this.output = output;
		}

		[Fact]
		public void when_getting_contents_then_includes_reference_assembly()
		{
			var result = Builder.BuildScenario(nameof(given_a_packaging_project_with_reference_assembly), target: "GetPackageContents", output: output);

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\portable45-net45+win8+wp8+wpa81\b.dll",
			}));
			Assert.Contains(result.Items, item => item.Matches(new
			{
				Kind = PackageItemKind.Lib,
				Identity = @"obj\a\Profile259\bin\b.dll",
			}));
			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\net46\b.dll",
			}));
		}
	}
}
