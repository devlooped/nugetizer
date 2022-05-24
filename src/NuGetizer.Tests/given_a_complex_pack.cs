using System.IO;
using System.Linq;
using Microsoft.Build.Execution;
using NuGet.Frameworks;
using Xunit;
using Xunit.Abstractions;

namespace NuGetizer
{
    public class given_a_complex_pack
    {
        ITestOutputHelper output;

        public given_a_complex_pack(ITestOutputHelper output)
        {
            this.output = output;
            Builder.BuildScenario(nameof(given_a_complex_pack), target: "Restore", output: output)
                .AssertSuccess(output);
        }

        [Fact]
        public void when_getting_package_target_path_then_gets_package_metadata()
        {
            var result = Builder.BuildScenario(nameof(given_a_complex_pack), new { Configuration = "Release" }, projectName: "a", target: "GetPackageTargetPath", output: output);

            result.AssertSuccess(output);

            var metadata = result.Items.FirstOrDefault();

            Assert.NotNull(metadata);
            Assert.Equal("A", metadata.GetMetadata("PackageId"));
            Assert.Equal("1.0.0", metadata.GetMetadata("Version"));
            Assert.Equal("NuGet", metadata.GetMetadata("Authors"));
        }

        [Fact]
        public void when_preparing_a_then_contains_assemblies_and_direct_dependency()
        {
            var result = Builder.BuildScenario(nameof(given_a_complex_pack), new { Configuration = "Release" }, projectName: "a", target: "GetPackageContents", output: output);

            result.AssertSuccess(output);

            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/net472/a.dll",
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/net472/a.xml",
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/net472/e.dll",
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/net472/e.xml",
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackFolder = PackFolderKind.Metadata,
                Filename = "A",
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                Identity = "B",
                PackFolder = PackFolderKind.Dependency,
                Version = "2.0.0",
            }));
        }

        [Fact]
        public void when_preparing_b_then_contains_assemblies_and_direct_dependency()
        {
            var result = Builder.BuildScenario(nameof(given_a_complex_pack), new { Configuration = "Release" }, projectName: "b", target: "GetPackageContents", output: output);

            result.AssertSuccess(output);

            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/net472/b.dll",
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/net472/b.xml",
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/net472/d.dll",
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/net472/d.xml",
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackFolder = PackFolderKind.Metadata,
                Filename = "B",
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                Identity = "C",
                PackFolder = PackFolderKind.Dependency,
                Version = "3.0.0",
            }));
        }

        [Fact]
        public void when_preparing_c_then_contains_external_dependency()
        {
            var result = Builder.BuildScenario(nameof(given_a_complex_pack), new { Configuration = "Release" }, projectName: "c", target: "GetPackageContents", output: output);

            result.AssertSuccess(output);

            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/net472/c.dll",
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/net472/c.xml",
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackFolder = PackFolderKind.Metadata,
                Filename = "C",
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                Identity = "Foo",
                PackFolder = PackFolderKind.Dependency,
                Version = "1.0.0",
            }));
        }

        [Fact]
        public void when_preparing_d_without_package_id_then_does_not_set_package_path()
        {
            var result = Builder.BuildScenario(nameof(given_a_complex_pack), projectName: "d", target: "GetPackageContents", output: output);

            result.AssertSuccess(output);

            Assert.Contains(result.Items, item => item.Matches(new
            {
                Filename = "d",
                Extension = ".dll",
                PackFolder = PackFolderKind.Lib,
                PackagePath = "",
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                Filename = "d",
                Extension = ".xml",
                PackFolder = PackFolderKind.Lib,
                PackagePath = "",
            }));
        }

        [Fact]
        public void when_packing_a_then_contains_assemblies_and_direct_dependency()
        {
            var result = Builder.BuildScenario(nameof(given_a_complex_pack), projectName: "a", target: "Pack", output: output);

            result.AssertSuccess(output);

            var manifest = result.Items[0].GetManifest();

            Assert.Contains(manifest.Files, file => file.Target == @"lib\net472\a.dll");
            Assert.Contains(manifest.Files, file => file.Target == @"lib\net472\a.xml");
            Assert.Contains(manifest.Files, file => file.Target == @"lib\net472\e.dll");
            Assert.Contains(manifest.Files, file => file.Target == @"lib\net472\e.xml");

            Assert.Contains(manifest.Metadata.DependencyGroups, group =>
                group.TargetFramework.Equals(NuGetFramework.Parse("net472")) &&
                group.Packages.Any(dep => dep.Id == "B" && dep.VersionRange.OriginalString == "2.0.0"));
        }

        [Fact]
        public void when_packing_b_then_contains_assemblies_and_direct_dependency()
        {
            var result = Builder.BuildScenario(nameof(given_a_complex_pack), projectName: "b", target: "Pack", output: output);

            result.AssertSuccess(output);

            var manifest = result.Items[0].GetManifest();

            Assert.Contains(manifest.Files, file => file.Target == @"lib\net472\b.dll");
            Assert.Contains(manifest.Files, file => file.Target == @"lib\net472\b.xml");
            Assert.Contains(manifest.Files, file => file.Target == @"lib\net472\d.dll");
            Assert.Contains(manifest.Files, file => file.Target == @"lib\net472\d.xml");

            Assert.Contains(manifest.Metadata.DependencyGroups, group =>
                group.TargetFramework.Equals(NuGetFramework.Parse("net472")) &&
                group.Packages.Any(dep => dep.Id == "C" && dep.VersionRange.OriginalString == "3.0.0"));
        }

        [Fact]
        public void when_packing_c_then_contains_external_dependency()
        {
            var result = Builder.BuildScenario(nameof(given_a_complex_pack), projectName: "c", target: "Pack", output: output);

            result.AssertSuccess(output);

            var manifest = result.Items[0].GetManifest();

            Assert.Contains(manifest.Files, file => file.Target == @"lib\net472\c.dll");
            Assert.Contains(manifest.Files, file => file.Target == @"lib\net472\c.xml");

            Assert.Contains(manifest.Metadata.DependencyGroups, group =>
                group.TargetFramework.Equals(NuGetFramework.Parse("net472")) &&
                group.Packages.Any(dep => dep.Id == "Foo" && dep.VersionRange.OriginalString == "1.0.0"));
        }

        [Fact]
        public void when_packing_d_without_package_id_then_target_is_skipped()
        {
            var result = Builder.BuildScenario(nameof(given_a_complex_pack), projectName: "d", target: "Pack", output: output);

            Assert.Equal(TargetResultCode.Skipped, result.ResultCode);
        }

        [Fact]
        public void when_pack_with_emit_nuspec_but_not_package_then_creates_nuspec_but_not_package()
        {
            var result = Builder.BuildScenario(nameof(given_a_complex_pack),
                new
                {
                    EmitNuspec = "true",
                    EmitPackage = "false",
                    BaseOutputPath = Builder.TestOutputPath(),
                },
                projectName: "a",
                target: "Pack", output: output);

            result.AssertSuccess(output);
            Assert.Single(result.Items);

            var pkgFile = result.Items[0].GetMetadata("FullPath");

            Assert.False(File.Exists(pkgFile));
            Assert.True(File.Exists(Path.Combine(Path.GetDirectoryName(pkgFile), "A.nuspec")));
        }

        [Fact]
        public void when_pack_with_emit_package_but_not_nuspec_then_creates_package_but_not_nuspec()
        {
            var result = Builder.BuildScenario(nameof(given_a_complex_pack),
                new
                {
                    EmitNuspec = "false",
                    EmitPackage = "true",
                    BaseOutputPath = Builder.TestOutputPath(),
                },
                projectName: "a",
                target: "Pack", output: output);

            result.AssertSuccess(output);
            Assert.Single(result.Items);

            var pkgFile = result.Items[0].GetMetadata("FullPath");

            Assert.True(File.Exists(pkgFile));
            Assert.False(File.Exists(Path.Combine(Path.GetDirectoryName(pkgFile), "A.nuspec")));
        }
    }
}
