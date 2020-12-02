using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.Build.Execution;
using NuGet.Packaging;
using Xunit;
using Xunit.Abstractions;

namespace NuGetizer
{
    public class InlineProjectTests
    {
        ITestOutputHelper output;

        public InlineProjectTests(ITestOutputHelper output) => this.output = output;

        [Fact]
        public void when_is_packable_true_then_package_id_defaults_to_assembly_name()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <IsPackable>true</IsPackable>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
</Project>", output: output);

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackFolder = PackFolderKind.Metadata
            }));
        }

        [Fact]
        public void when_is_packable_true_but_packageid_reset_to_empty_then_fails()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <IsPackable>true</IsPackable>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <Target Name='BeforeEverything' BeforeTargets='PrepareForBuild'>
    <!-- We need to try harder to reset it to empty, since it's defaulted 
         in this case to the AssemblyName automatically -->
    <PropertyGroup>
        <PackageId />
    </PropertyGroup>
  </Target>
</Project>", output: output);

            Assert.Equal(TargetResultCode.Failure, result.ResultCode);
            // Next best to checking the full string. No way I could find to 
            // get the actually build error code.
            Assert.Contains("PackageId", result.ToString());
            Assert.Contains("IsPackable", result.ToString());
        }

        [Fact]
        public void when_no_is_packable_and_no_package_id_then_defaults_to_non_packable()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
</Project>", output: output);

            result.AssertSuccess(output);
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                PackFolder = PackFolderKind.Metadata
            }));
        }

        [Fact]
        public void when_getting_package_contents_then_includes_output_assembly()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
</Project>", output: output);

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                Extension = ".dll",
                PackFolder = PackFolderKind.Lib
            }));
        }

        [Fact]
        public void when_include_outputs_in_package_is_false_then_does_not_include_main_assembly()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackBuildOutput>false</PackBuildOutput>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
</Project>", output: output);

            result.AssertSuccess(output);
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                Extension = ".dll",
                PackFolder = PackFolderKind.Lib
            }));
        }

        [Fact]
        public void when_getting_package_contents_then_includes_symbols()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
</Project>", output: output);

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                Extension = ".pdb",
            }));
        }

        [Fact]
        public void when_include_symbols_in_package_is_true_but_include_outputs_is_false_then_does_not_include_symbols()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackBuildOutput>false</PackBuildOutput>
    <PackSymbols>true</PackSymbols>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
</Project>", output: output);

            result.AssertSuccess(output);
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                Extension = ".pdb",
            }));
        }

        [Fact]
        public void when_include_symbols_in_package_is_false_then_does_not_include_symbols()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackSymbols>false</PackSymbols>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
</Project>", output: output);

            result.AssertSuccess(output);
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                Extension = ".pdb",
            }));
        }

        [Fact]
        public void when_getting_package_contents_then_includes_xmldoc()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackSymbols>false</PackSymbols>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
</Project>", output: output);

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                Extension = ".xml",
                PackFolder = PackFolderKind.Lib,
            }));
        }

        [Fact]
        public void when_include_output_in_package_is_false_then_does_not_include_xmldoc()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackBuildOutput>false</PackBuildOutput>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
</Project>", output: output);


            result.AssertSuccess(output);
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                Extension = ".xml",
                PackFolder = PackFolderKind.Lib,
            }));
        }

        [Fact]
        public void when_getting_package_contents_then_annotates_items_with_package_id()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Foo</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
</Project>", output: output);

            result.AssertSuccess(output);
            Assert.All(result.Items, item => Assert.Equal("Foo", item.GetMetadata("PackageId")));
        }

        [Fact]
        public void when_getting_package_contents_then_includes_framework_reference()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Foo</PackageId>
    <TargetFramework>net472</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include='PresentationFramework' />
  </ItemGroup>
</Project>", output: output);

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                Identity = "PresentationFramework",
                PackFolder = PackFolderKind.FrameworkReference,
            }));
        }

        [Fact]
        public void when_include_framework_references_in_package_is_false_then_does_not_include_framework_reference()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Foo</PackageId>
    <PackFrameworkReferences>false</PackFrameworkReferences>
    <TargetFramework>net472</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include='PresentationFramework' />
  </ItemGroup>
</Project>", output: output);

            result.AssertSuccess(output);
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                Identity = "PresentationFramework",
                PackFolder = PackFolderKind.FrameworkReference,
            }));
        }

        [Fact]
        public void when_updating_package_item_metadata_then_updates_metadata()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Foo</PackageId>
    <PackBuildOutput>false</PackBuildOutput>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageMetadata Include='Foo'>
      <Description>ItemDescription</Description>
    </PackageMetadata>
  </ItemGroup>
</Project>", output: output);

            result.AssertSuccess(output);

            Assert.Single(result.Items);
            var metadata = result.Items[0];

            Assert.Equal("ItemDescription", metadata.GetMetadata("Description"));
        }

        [Fact]
        public void when_updating_package_metadata_via_target_then_updates_metadata()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Foo</PackageId>
    <PackBuildOutput>false</PackBuildOutput>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <Target Name='BeforeGetMetadata' BeforeTargets='GetPackageMetadata'>
    <PropertyGroup>
      <Description>PropertyDescription</Description>
    </PropertyGroup>
  </Target>
</Project>", output: output);

            result.AssertSuccess(output);

            Assert.Single(result.Items);
            var metadata = result.Items[0];

            Assert.Equal("PropertyDescription", metadata.GetMetadata("Description"));
        }

        [Fact]
        public void when_setting_metadata_property_then_updates_metadata()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Foo</PackageId>
    <PackBuildOutput>false</PackBuildOutput>
    <Title>MyPackage</Title>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <Target Name='BeforeGetMetadata' BeforeTargets='GetPackageMetadata'>
    <PropertyGroup>
      <Description>PropertyDescription</Description>
    </PropertyGroup>
  </Target>
</Project>", output: output);

            result.AssertSuccess(output);

            Assert.Single(result.Items);
            var metadata = result.Items[0];

            Assert.Equal("MyPackage", metadata.GetMetadata("Title"));
        }

        [Fact]
        public void when_package_reference_has_metadata_then_inferred_package_references_has_same_metadata()
        {
            var project = @"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackBuildOutput>false</PackBuildOutput>
    <Title>MyPackage</Title>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Microsoft.CodeAnalysis' Version='3.8.0' MyMetadata='Foo' PrivateAssets='all' />
  </ItemGroup>
</Project>";

            var result = Builder.BuildProject(project, "_CollectPrimaryOutputDependencies", output: output);
            result.AssertSuccess(output);

            var metadata = result.Items.FirstOrDefault(i => i.ItemSpec == "Microsoft.CodeAnalysis.Common");

            Assert.NotNull(metadata);
            Assert.Equal("Foo", metadata.GetMetadata("MyMetadata"));
        }

        [Fact]
        public void when_referencing_package_reference_file_then_it_requires_generate_path_property()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <Title>MyPackage</Title>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='ThisAssembly' Version='1.0.0' GeneratePathProperty='false' />
  </ItemGroup>
  <ItemGroup>
    <PackageFile Include='icon-128.png' PackageReference='ThisAssembly' />
  </ItemGroup>
</Project>", output: output);

            Assert.Equal(TargetResultCode.Failure, result.ResultCode);

            // Next best to checking the full string. No way I could find to 
            // get the actually build error code.
            Assert.Contains("GeneratePathProperty", result.ToString());
        }

        [Fact]
        public void when_referencing_package_reference_file_then_resolves_to_package_path()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <Title>MyPackage</Title>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='ThisAssembly' Version='1.0.0' GeneratePathProperty='true' />
  </ItemGroup>
  <ItemGroup>
    <PackageFile Include='icon-128.png' PackageReference='ThisAssembly' />
  </ItemGroup>
</Project>", output: output);

            result.AssertSuccess(output);

            var icon = result.Items.FirstOrDefault(i => i.ItemSpec.EndsWith("icon-128.png"));

            Assert.NotNull(icon);
            Assert.True(File.Exists(icon.GetMetadata("FullPath")));
        }

        [Fact]
        public void when_referencing_package_reference_file_then_can_use_inference_items()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <Title>MyPackage</Title>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='ThisAssembly' Version='1.0.0' GeneratePathProperty='true' />
  </ItemGroup>
  <ItemGroup>
    <Content Include='icon-128.png' PackageReference='ThisAssembly' />
  </ItemGroup>
</Project>", output: output);

            result.AssertSuccess(output);

            var icon = result.Items.FirstOrDefault(i => i.ItemSpec.EndsWith("icon-128.png"));

            Assert.NotNull(icon);
            Assert.True(File.Exists(icon.GetMetadata("FullPath")));
        }

        [Fact]
        public void when_pack_on_build_multitargeting_then_contains_all_targets()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <IsPackable>true</IsPackable>
    <TargetFrameworks>net472;netstandard2.0</TargetFrameworks>
    <PackOnBuild>true</PackOnBuild>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Microsoft.NETFramework.ReferenceAssemblies' Version='1.0.0' />
  </ItemGroup>
</Project>", "Build,GetPackageTargetPath", output);
            
            result.AssertSuccess(output);

            Assert.Single(result.Items);
            var packagePath = result.Items[0].GetMetadata("FullPath");

            Assert.True(File.Exists(packagePath));

            using var package = ZipFile.OpenRead(packagePath);
            var files = package.GetFiles().ToArray();

            Assert.Contains(files, file => file.StartsWith("lib/net472"));
            Assert.Contains(files, file => file.StartsWith("lib/netstandard2.0"));
        }
    }
}
