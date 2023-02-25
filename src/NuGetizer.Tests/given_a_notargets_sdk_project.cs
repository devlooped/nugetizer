using Xunit;
using Xunit.Abstractions;

namespace NuGetizer
{
    public class given_a_notargets_sdk_project
    {
        ITestOutputHelper output;

        public given_a_notargets_sdk_project(ITestOutputHelper output) => this.output = output;

        [Fact]
        public void cam_reference_packaging_project()
        {
            var result = Builder.BuildProjects(
                "GetPackageContents", output, null,
                ("main.msbuildproj", @"
<Project Sdk='Microsoft.Build.NoTargets/3.7.0'>
  <PropertyGroup>
    <PackageId>Foo</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Newtonsoft.Json' Version='12.0.3' />
    <ProjectReference Include='other.msbuildproj' />
  </ItemGroup>
</Project>"),
                ("other.msbuildproj", @"
<Project Sdk='Microsoft.Build.NoTargets/3.7.0'>
  <PropertyGroup>
    <PackageId>Bar</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Castle.Core' Version='4.4.1' />
  </ItemGroup>
</Project>"));

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                Identity = "Foo",
                PackFolder = PackFolderKind.Metadata
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                Identity = "Newtonsoft.Json",
                PackFolder = PackFolderKind.Dependency
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                Identity = "Bar",
                PackFolder = PackFolderKind.Dependency
            }));
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                Identity = "Castle.Core",
                PackageId = "Foo",
                PackFolder = PackFolderKind.Dependency
            }));
        }

        [Fact]
        public void cam_be_referenced()
        {
            var result = Builder.BuildProjects(
                "GetPackageContents", output, null,
                ("main.csproj", @"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Foo</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include='other.msbuildproj' />
  </ItemGroup>
</Project>"),
                ("other.msbuildproj", @"
<Project Sdk='Microsoft.Build.NoTargets/3.7.0'>
  <PropertyGroup>
    <PackageId>Bar</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Castle.Core' Version='4.4.1' />
  </ItemGroup>
</Project>"));

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                Identity = "Foo",
                PackFolder = PackFolderKind.Metadata
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                Identity = "Bar",
                PackFolder = PackFolderKind.Dependency
            }));
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                Identity = "Castle.Core",
                PackageId = "Foo",
                PackFolder = PackFolderKind.Dependency
            }));
        }
    }
}
