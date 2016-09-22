using System.Linq;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using NuGet.Build.Packaging.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace NuGet.Build.Packaging
{
	public class given_a_complex_pack
	{
		ITestOutputHelper output;

		public given_a_complex_pack(ITestOutputHelper output)
		{
			this.output = output;
		}

		[Fact]
		public void when_packing_a_then_contains_assemblies_and_direct_dependency()
		{
			var result = Builder.BuildScenario(nameof(given_a_complex_pack), new { Configuration = "Release" }, projectName: "a", target: "_FilterPackageDependencyContents", output: output);

			Assert.Equal(TargetResultCode.Success, result.ResultCode);

			Assert.Collection(result.Items,
				item => item.Matches(new
				{
					PackagePath = @"lib\net45\a.dll",
				}),
				item => item.Matches(new
				{
					PackagePath = @"lib\net45\a.xml",
				}),
				item => item.Matches(new
				{
					PackagePath = @"lib\net45\e.dll",
				}),
				item => item.Matches(new
				{
					PackagePath = @"lib\net45\e.xml",
				}),
				item => item.Matches(new
				{
					Kind = "Metadata",
					Filename = "A",
				}),
				item => item.Matches(new
				{
					Kind = "Dependency",
					PackageId = "B",
					Version = "2.0.0",
				})
			);
		}

		[Fact]
		public void when_packing_b_then_contains_assemblies_and_direct_dependency()
		{
			var result = Builder.BuildScenario(nameof(given_a_complex_pack), new { Configuration = "Release" }, projectName: "b", target: "_FilterPackageDependencyContents", output: output);

			Assert.Equal(TargetResultCode.Success, result.ResultCode);

			Assert.Collection(result.Items,
				item => item.Matches(new
				{
					PackagePath = @"lib\net45\b.dll",
				}),
				item => item.Matches(new
				{
					PackagePath = @"lib\net45\b.xml",
				}),
				item => item.Matches(new
				{
					PackagePath = @"lib\net45\d.dll",
				}),
				item => item.Matches(new
				{
					PackagePath = @"lib\net45\d.xml",
				}),
				item => item.Matches(new
				{
					Kind = "Metadata",
					Filename = "B",
				}),
				item => item.Matches(new
				{
					Kind = "Dependency",
					PackageId = "C",
					Version = "3.0.0",
				})
			);
		}

		[Fact]
		public void when_packing_c_then_contains_external_dependency()
		{
			var result = Builder.BuildScenario(nameof(given_a_complex_pack), new { Configuration = "Release" }, projectName: "c", target: "_FilterPackageDependencyContents", output: output);

			Assert.Equal(TargetResultCode.Success, result.ResultCode);

			Assert.Collection(result.Items,
				item => item.Matches(new
				{
					PackagePath = @"lib\net45\c.dll",
				}),
				item => item.Matches(new
				{
					PackagePath = @"lib\net45\c.xml",
				}),
				item => item.Matches(new
				{
					Kind = "Metadata",
					Filename = "C",
				}),
				item => item.Matches(new
				{
					Kind = "Dependency",
					PackageId = "Foo",
					Version = "1.0.0",
				})
			);
		}

		[Fact]
		public void when_packing_d_without_package_id_then_does_not_set_package_path()
		{
			var result = Builder.BuildScenario(nameof(given_a_complex_pack), new { Configuration = "Release" }, projectName: "d", target: "_FilterPackageDependencyContents", output: output);

			Assert.Equal(TargetResultCode.Success, result.ResultCode);

			Assert.Collection(result.Items,
				item => item.Matches(new
				{
					Filename = "d",
					Extension = ".dll",
					Kind = "Lib",
					PackagePath = "",
				}),
				item => item.Matches(new
				{
					Filename = "d",
					Extension = ".xml",
					Kind = "Lib",
					PackagePath = "",
				})
			);
		}
	}
}
