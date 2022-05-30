using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace NuGetizer
{
    public class given_a_packaging_project
    {
        ITestOutputHelper output;

        public given_a_packaging_project(ITestOutputHelper output)
        {
            this.output = output;
            Builder.BuildScenario(nameof(given_a_packaging_project), target: "Restore")
                .AssertSuccess(output);
        }

        [Fact]
        public void when_getting_contents_then_includes_referenced_project_outputs()
        {
            var result = Builder.BuildScenario(nameof(given_a_packaging_project), output: output);

            result.AssertSuccess(output);

            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/net46/b.dll",
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/net46/b.xml",
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/net46/b.dll",
            }));
        }

        [Fact]
        public void when_getting_contents_then_can_augment_package_metadata()
        {
            var result = Builder.BuildScenario(nameof(given_a_packaging_project), output: output);

            result.AssertSuccess(output);

            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackFolder = PackFolderKind.Metadata,
                Foo = "Bar",
            }));
        }

        [Fact]
        public void when_getting_contents_then_includes_referenced_project_satellite_assembly()
        {
            var result = Builder.BuildScenario(nameof(given_a_packaging_project), output: output);

            result.AssertSuccess(output);

            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/net46/es-AR/b.resources.dll",
            }));
        }

        [Fact]
        public void when_getting_contents_then_includes_referenced_project_dependencies()
        {
            var result = Builder.BuildScenario(nameof(given_a_packaging_project), output: output);

            result.AssertSuccess(output);

            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/net46/d.dll",
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/net46/d.dll",
            }));
        }

        [Fact]
        public void when_getting_contents_then_includes_referenced_project_dependency_satellite_assembly()
        {
            var result = Builder.BuildScenario(nameof(given_a_packaging_project), output: output);

            result.AssertSuccess(output);

            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/net46/es-AR/d.resources.dll",
            }));
        }

        [Fact]
        public void when_getting_contents_then_includes_referenced_packagable_project_as_dependency()
        {
            var result = Builder.BuildScenario(nameof(given_a_packaging_project), output: output);

            result.AssertSuccess(output);

            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackFolder = PackFolderKind.Dependency,
                Identity = "E",
            }));
        }

        [Fact]
        public void when_getting_contents_then_does_not_include_referenced_project_nuget_assembly_reference()
        {
            var result = Builder.BuildScenario(nameof(given_a_packaging_project), output: output);

            result.AssertSuccess(output);

            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/net46/Newtonsoft.Json.dll",
            }));
        }

        [Fact]
        public void when_getting_contents_from_packaging_project_then_referenced_outputs_have_original_tfm_path()
        {
            var result = Builder.BuildScenario(nameof(given_a_packaging_project), output: output);

            result.AssertSuccess(output);

            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/net472/c.dll",
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/net472/c.xml",
            }));
        }

        [Fact]
        public void when_getting_contents_then_transitive_content_is_made_full_path()
        {
            var result = Builder.BuildScenario(nameof(given_a_packaging_project), output: output);

            result.AssertSuccess(output);

            Assert.Contains(result.Items, item => item.Matches(new
            {
                Filename = "Readme",
                PackFolder = "None",
            }) && Path.IsPathRooted(item.ItemSpec));
        }

        [Fact]
        public void when_getting_contents_then_transitive_content_can_opt_out_of_full_path()
        {
            var result = Builder.BuildScenario(nameof(given_a_packaging_project), properties: new { AddAsIs = "true" }, output: output);

            result.AssertSuccess(output);

            Assert.Contains(result.Items, item => item.Matches(new
            {
                Filename = "as-is",
                PackFolder = "None",
            }) && !Path.IsPathRooted(item.ItemSpec));
        }

        [Fact]
        public void when_packing_then_succeeeds()
        {
            var result = Builder.BuildScenario(nameof(given_a_packaging_project), target: "Pack", output: output);

            result.AssertSuccess(output);
        }

        [Fact]
        public void when_framework_specific_then_retargets_direct_and_referenced_content()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.Build.NoTargets/3.5.0'>
  <PropertyGroup>
    <PackageId>Packer</PackageId>
    <TargetFramework>net6.0</TargetFramework>
    <PackFolder>build</PackFolder>
    <BuildOutputFrameworkSpecific>true</BuildOutputFrameworkSpecific>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include='Tasks.csproj' PrivateAssets='all' />
    <!-- NOTE: for other stuff, we can put it in the same folders as the build output by 
         specifying PackFolder and FrameworkSpecific to match the project's. -->
    <None Include='readme.md' PackFolder='$(PackFolder)' FrameworkSpecific='$(BuildOutputFrameworkSpecific)' />
  </ItemGroup>
</Project>",
                "GetPackageContents", output,
                files: new[]
                {
                    ("Tasks.csproj", @"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Microsoft.Build.Tasks.Core' Version='16.6.0' />
  </ItemGroup>
</Project>"),
                    ("readme.md", "# readme")
                });

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"build/net6.0/readme.md",
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"build/net6.0/Tasks.dll",
            }));
        }

        [Fact]
        public void when_referenced_project_has_packfolder_then_preserves_it()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.Build.NoTargets/3.5.0'>
  <PropertyGroup>
    <PackageId>Packer</PackageId>
    <TargetFramework>net6.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include='Transitive.csproj' />
    <ProjectReference Include='Build.csproj' />
  </ItemGroup>
</Project>",
                "GetPackageContents", output,
                files: new[]
                {
                    ("Transitive.csproj", @"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <PackFolder>buildTransitive</PackFolder>
  </PropertyGroup>
</Project>"),
                    ("Build.csproj", @"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <PackFolder>build</PackFolder>
  </PropertyGroup>
</Project>"),
                });

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"buildTransitive/Transitive.dll",
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"build/Build.dll",
            }));
        }


        [Fact]
        public void when_project_reference_packfolder_additional_properties_then_overrides_project_pack_folder()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.Build.NoTargets/3.5.0'>
  <PropertyGroup>
    <PackageId>Packer</PackageId>
    <TargetFramework>net6.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include='Library.csproj' AdditionalProperties='PackFolder=tools/net6.0' />
  </ItemGroup>
</Project>",
                "GetPackageContents", output,
                files: ("Library.csproj", @"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <PackFolder>lib</PackFolder>
  </PropertyGroup>
</Project>"));

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"tools/net6.0/Library.dll",
            }));
        }

        [Fact]
        public void when_project_reference_packfolder_then_overrides_project_pack_folder()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.Build.NoTargets/3.5.0'>
  <PropertyGroup>
    <PackageId>Packer</PackageId>
    <TargetFramework>net6.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include='Library.csproj' PackFolder='tools/net6.0' />
  </ItemGroup>
</Project>",
                "GetPackageContents", output,
                files: ("Library.csproj", @"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <PackFolder>lib</PackFolder>
  </PropertyGroup>
</Project>"));

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"tools/net6.0/Library.dll",
            }));
        }

        [Fact]
        public void when_pack_folder_build_then_none_packs_as_build()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.Build.NoTargets/3.5.0'>
  <PropertyGroup>
    <PackageId>Packer</PackageId>
    <TargetFramework>net6.0</TargetFramework>
    <PackFolder>build</PackFolder>
    <!-- Only needed since for scenarios we set this to false. -->
    <EnableDefaultItems>true</EnableDefaultItems>
  </PropertyGroup>
</Project>",
                "GetPackageContents", output,
                files: ("Packer.targets", @"<Project />"));

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"build/Packer.targets",
            }));
        }

        [Fact]
        public void when_readme_found_but_pack_readme_false_then_does_not_add_it()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.Build.NoTargets/3.5.0'>
  <PropertyGroup>
    <PackageId>Packer</PackageId>
    <TargetFramework>net6.0</TargetFramework>
    <!-- Only needed since for scenarios we set this to false. -->
    <EnableDefaultItems>true</EnableDefaultItems>
    <PackReadme>false</PackReadme>
  </PropertyGroup>
</Project>",
                "GetPackageContents", output,
                files: ("readme.md", @"# readme"));

            result.AssertSuccess(output);

            // Assert the readme file is not added to the package
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                PackagePath = @"readme.md",
            }));

            // Assert the package metadata is not present
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                Identity = "Packer",
                Readme = "readme.md",
            }));
        }

        [Fact]
        public void when_readme_found_but_project_not_packable_then_does_not_add_content()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.Build.NoTargets/3.5.0'>
  <PropertyGroup>
    <IsPackable>false</IsPackable>
    <TargetFramework>net6.0</TargetFramework>
    <!-- Only needed since for scenarios we set this to false. -->
    <EnableDefaultItems>true</EnableDefaultItems>
  </PropertyGroup>
</Project>",
                "GetPackageContents", output,
                files: ("readme.md", @"# readme"));

            result.AssertSuccess(output);

            // Assert the readme file is not added to the package
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                PackagePath = @"readme.md",
            }));

            // Assert the package metadata is not present
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                Identity = "Packer",
                Readme = "readme.md",
            }));
        }

        [Fact]
        public void when_readme_found_then_adds_metadata_and_content()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.Build.NoTargets/3.5.0'>
  <PropertyGroup>
    <PackageId>Packer</PackageId>
    <TargetFramework>net6.0</TargetFramework>
    <!-- Only needed since for scenarios we set this to false. -->
    <EnableDefaultItems>true</EnableDefaultItems>
  </PropertyGroup>
</Project>",
                "GetPackageContents", output,
                files: ("readme.md", @"# readme"));

            result.AssertSuccess(output);

            // Assert the readme file is added to the package
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"readme.md",
            }));

            // Assert the package metadata is present too
            Assert.Contains(result.Items, item => item.Matches(new
            {
                Identity = "Packer",
                Readme = "readme.md",
            }));
        }

        [Fact]
        public void when_readme_custom_extension_specified_then_adds_metadata_and_content()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.Build.NoTargets/3.5.0'>
  <PropertyGroup>
    <PackageId>Packer</PackageId>
    <TargetFramework>net6.0</TargetFramework>
    <!-- Only needed since for scenarios we set this to false. -->
    <EnableDefaultItems>true</EnableDefaultItems>
    <PackageReadmeFile>readme.txt</PackageReadmeFile>
  </PropertyGroup>
</Project>",
                "GetPackageContents", output,
                files: ("readme.txt", @"readme"));

            result.AssertSuccess(output);

            // Assert the readme file is added to the package
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"readme.txt",
            }));

            // Assert the package metadata is present too
            Assert.Contains(result.Items, item => item.Matches(new
            {
                Identity = "Packer",
                Readme = "readme.txt",
            }));
        }
    }
}
