using Microsoft.Build.Execution;
using Xunit;
using Xunit.Abstractions;

namespace NuGetizer
{
    public class given_projectreferences
    {
        ITestOutputHelper output;

        public given_projectreferences(ITestOutputHelper output) => this.output = output;

        [Fact]
        public void when_privateassets_all_then_packs_library()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Analyzer</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
    <PackFolder>analyzers\cs</PackFolder>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include='Library.csproj' PrivateAssets='all' />
  </ItemGroup>
</Project>",
                "GetPackageContents", output,
                files: ("Library.csproj", @"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Library</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
</Project>"));

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"analyzers/cs/Library.dll",
            }));
        }

        [Fact]
        public void when_privateassets_on_explicit_PackFolder_then_packs_library()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Analyzer</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
    <PackFolder>analyzers/cs</PackFolder>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include='Library.csproj' PrivateAssets='all' />
  </ItemGroup>
</Project>",
                "GetPackageContents", output,
                files: ("Library.csproj", @"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Library</PackageId>
    <PackFolder>build</PackFolder>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
</Project>"));

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"analyzers/cs/Library.dll",
            }));
        }

        [Fact]
        public void when_privateassets_all_then_packs_transitive_libraries()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Library</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Prism.Forms' Version='7.2.0.1422' PrivateAssets='all' />
  </ItemGroup>
</Project>",
                "GetPackageContents", output);

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/netstandard2.0/Prism.dll",
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/netstandard2.0/Prism.Forms.dll",
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/netstandard2.0/Xamarin.Forms.Core.dll",
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/netstandard2.0/Xamarin.Forms.Platform.dll",
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/netstandard2.0/Xamarin.Forms.Xaml.dll",
            }));
        }

        [Fact]
        public void when_privateassets_all_and_pack_false_then_does_not_pack_transitively()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Library</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Prism.Forms' Version='7.2.0.1422' PrivateAssets='all' Pack='false' />
  </ItemGroup>
</Project>",
                "GetPackageContents", output);

            result.AssertSuccess(output);
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/netstandard2.0/Prism.dll",
            }));
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/netstandard2.0/Prism.Forms.dll",
            }));
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/netstandard2.0/Xamarin.Forms.Core.dll",
            }));
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/netstandard2.0/Xamarin.Forms.Platform.dll",
            }));
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/netstandard2.0/Xamarin.Forms.Xaml.dll",
            }));
        }

        [Fact]
        public void when_privateassets_pack_false_then_does_not_pack()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Library</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Newtonsoft.Json' Version='1.0.0' PrivateAssets='all' Pack='false' />
  </ItemGroup>
</Project>",
                "GetPackageContents", output);

            result.AssertSuccess(output);
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/netstandard2.0/Newtonsoft.Json.dll",
            }));
        }

        [Fact]
        public void when_build_kind_then_does_not_pack_msbuild()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Library</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
    <PackFolder>build</PackFolder>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Microsoft.Build.Tasks.Core' Version='16.6.0' />
  </ItemGroup>
</Project>",
                "GetPackageContents", output);

            result.AssertSuccess(output);
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                Identity = "Microsoft.Build.Tasks.Core",
                PackFolder = PackFolderKind.Dependency,
            }));
        }

        [Fact]
        public void when_build_kind_and_explicit_pack_then_packs_msbuild()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Library</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
    <PackFolder>build</PackFolder>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Microsoft.Build.Tasks.Core' Version='16.6.0' Pack='true' />
  </ItemGroup>
</Project>",
                "GetPackageContents", output);

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                Identity = "Microsoft.Build.Tasks.Core",
                PackFolder = PackFolderKind.Dependency,
            }));
        }

        [Fact]
        public void when_projectreference_explicit_packfolder_then_specific_folder_is_packed()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.Build.NoTargets/3.0.4'>
  <PropertyGroup>
    <PackageId>Packer</PackageId>
    <TargetFramework>net5.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include='Library.csproj' AdditionalProperties='PackFolder=lib/net5.0/SpecificFolder' />
  </ItemGroup>
</Project>",
                "GetPackageContents", output,
                files: ("Library.csproj", @"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <TargetFramework>net472</TargetFramework>
  </PropertyGroup>
</Project>"));

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/net5.0/SpecificFolder/Library.dll",
            }));
        }

        [Fact]
        public void when_projectreference_has_packfolder_metadata_then_specific_folder_is_packed()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.Build.NoTargets/3.0.4'>
  <PropertyGroup>
    <PackageId>Packer</PackageId>
    <TargetFramework>net5.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include='Library.csproj' PackFolder='lib/net5.0/SpecificFolder' />
  </ItemGroup>
</Project>",
                "GetPackageContents", output,
                files: ("Library.csproj", @"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <TargetFramework>net472</TargetFramework>
  </PropertyGroup>
</Project>"));

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib/net5.0/SpecificFolder/Library.dll",
            }));
        }

    }
}
