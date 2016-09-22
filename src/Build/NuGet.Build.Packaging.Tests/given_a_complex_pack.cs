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
	public class given_a_complex_pack
	{
		ITestOutputHelper output;

		public given_a_complex_pack(ITestOutputHelper output)
		{
			this.output = output;
		}

		[Fact]
		public void when_preparing_a_then_contains_assemblies_and_direct_dependency()
		{
			var result = Builder.BuildScenario(nameof(given_a_complex_pack), new { Configuration = "Release" }, projectName: "a", target: "_PrepareForPack", output: output);

			Assert.Equal(TargetResultCode.Success, result.ResultCode);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\net45\a.dll",
			}));
			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\net45\a.xml",
			}));
			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\net45\e.dll",
			}));
			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\net45\e.xml",
			}));
			Assert.Contains(result.Items, item => item.Matches(new
			{
				Kind = "Metadata",
				Filename = "A",
			}));
			Assert.Contains(result.Items, item => item.Matches(new
			{
				Kind = "Dependency",
				PackageId = "B",
				Version = "2.0.0",
			}));
		}

		[Fact]
		public void when_preparing_b_then_contains_assemblies_and_direct_dependency()
		{
			var result = Builder.BuildScenario(nameof(given_a_complex_pack), new { Configuration = "Release" }, projectName: "b", target: "_PrepareForPack", output: output);

			Assert.Equal(TargetResultCode.Success, result.ResultCode);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\net45\b.dll",
			}));
			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\net45\b.xml",
			}));
			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\net45\d.dll",
			}));
			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\net45\d.xml",
			}));
			Assert.Contains(result.Items, item => item.Matches(new
			{
				Kind = "Metadata",
				Filename = "B",
			}));
			Assert.Contains(result.Items, item => item.Matches(new
			{
				Kind = "Dependency",
				PackageId = "C",
				Version = "3.0.0",
			}));
		}

		[Fact]
		public void when_preparing_c_then_contains_external_dependency()
		{
			var result = Builder.BuildScenario(nameof(given_a_complex_pack), new { Configuration = "Release" }, projectName: "c", target: "_PrepareForPack", output: output);

			Assert.Equal(TargetResultCode.Success, result.ResultCode);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\net45\c.dll",
			}));
			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\net45\c.xml",
			}));
			Assert.Contains(result.Items, item => item.Matches(new
			{
				Kind = "Metadata",
				Filename = "C",
			}));
			Assert.Contains(result.Items, item => item.Matches(new
			{
				Identity = "Foo",
				Kind = "Dependency",
				Version = "1.0.0",
			}));
		}

		[Fact]
		public void when_preparing_d_without_package_id_then_does_not_set_package_path()
		{
			var result = Builder.BuildScenario(nameof(given_a_complex_pack), new { Configuration = "Release" }, projectName: "d", target: "_PrepareForPack", output: output);

			Assert.Equal(TargetResultCode.Success, result.ResultCode);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				Filename = "d",
				Extension = ".dll",
				Kind = "Lib",
				PackagePath = "",
			}));
			Assert.Contains(result.Items, item => item.Matches(new
			{
				Filename = "d",
				Extension = ".xml",
				Kind = "Doc",
				PackagePath = "",
			}));
		}

		[Fact]
		public void when_packing_a_then_contains_assemblies_and_direct_dependency()
		{
			var result = Builder.BuildScenario(nameof(given_a_complex_pack), projectName: "a", target: "Pack", output: output);

			Assert.Equal(TargetResultCode.Success, result.ResultCode);

			var manifest = result.Items[0].GetManifest();

			Assert.Contains(manifest.Files, file => file.Target == @"lib\net45\a.dll");
			Assert.Contains(manifest.Files, file => file.Target == @"lib\net45\a.xml");
			Assert.Contains(manifest.Files, file => file.Target == @"lib\net45\e.dll");
			Assert.Contains(manifest.Files, file => file.Target == @"lib\net45\e.xml");

			Assert.Contains(manifest.Metadata.DependencyGroups, group =>
				group.TargetFramework.Equals(NuGetFramework.Parse("net45")) &&
				group.Packages.Any(dep => dep.Id == "B" && dep.VersionRange.OriginalString == "2.0.0"));
		}

		[Fact]
		public void when_packing_b_then_contains_assemblies_and_direct_dependency()
		{
			var result = Builder.BuildScenario(nameof(given_a_complex_pack), projectName: "b", target: "Pack", output: output);

			Assert.Equal(TargetResultCode.Success, result.ResultCode);

			var manifest = result.Items[0].GetManifest();

			Assert.Contains(manifest.Files, file => file.Target == @"lib\net45\b.dll");
			Assert.Contains(manifest.Files, file => file.Target == @"lib\net45\b.xml");
			Assert.Contains(manifest.Files, file => file.Target == @"lib\net45\d.dll");
			Assert.Contains(manifest.Files, file => file.Target == @"lib\net45\d.xml");

			Assert.Contains(manifest.Metadata.DependencyGroups, group =>
				group.TargetFramework.Equals(NuGetFramework.Parse("net45")) &&
				group.Packages.Any(dep => dep.Id == "C" && dep.VersionRange.OriginalString == "3.0.0"));
		}

		[Fact]
		public void when_packing_c_then_contains_external_dependency()
		{
			var result = Builder.BuildScenario(nameof(given_a_complex_pack), projectName: "c", target: "Pack", output: output);

			Assert.Equal(TargetResultCode.Success, result.ResultCode);

			var manifest = result.Items[0].GetManifest();

			Assert.Contains(manifest.Files, file => file.Target == @"lib\net45\c.dll");
			Assert.Contains(manifest.Files, file => file.Target == @"lib\net45\c.xml");

			Assert.Contains(manifest.Metadata.DependencyGroups, group =>
				group.TargetFramework.Equals(NuGetFramework.Parse("net45")) &&
				group.Packages.Any(dep => dep.Id == "Foo" && dep.VersionRange.OriginalString == "1.0.0"));
		}

		[Fact]
		public void when_packing_d_without_package_id_then_target_is_skipped()
		{
			var result = Builder.BuildScenario(nameof(given_a_complex_pack), projectName: "d", target: "Pack", output: output);

			Assert.Equal(TargetResultCode.Skipped, result.ResultCode);
		}
	}
}
