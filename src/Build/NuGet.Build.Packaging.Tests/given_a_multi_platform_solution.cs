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

			result.AssertSuccess(output);

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

			result.AssertSuccess(output);

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

			result.AssertSuccess(output);

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
		public void then_includes_platform_docs_from_before_get_package_contents()
		{
			var result = Builder.BuildScenario(nameof(given_a_multi_platform_solution), output: output);

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"docs\gettingstarted.html"
			}));
			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"docs\overview\index.html"
			}));
		}

	}
}
