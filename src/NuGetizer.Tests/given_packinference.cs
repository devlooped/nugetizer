using Microsoft.Build.Execution;
using Xunit;
using Xunit.Abstractions;

namespace NuGetizer
{
    public class given_packinference
    {
        ITestOutputHelper output;

        public given_packinference(ITestOutputHelper output) => this.output = output;

        [Fact]
        public void when_none_has_PackagePath_then_packs()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Library</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <None Include='Library.props' PackagePath='build\netstandard2.0\%(Filename)%(Extension)' />
  </ItemGroup>
</Project>",
                "GetPackageContents", output);

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"build\netstandard2.0\Library.props",
            }));
        }

        [Fact]
        public void when_PackContent_false_but_content_has_PackagePath_then_packs()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Library</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
    <PackContent>false</PackContent>
  </PropertyGroup>
  <ItemGroup>
    <Content Include='Library.props' PackagePath='build\netstandard2.0\%(Filename)%(Extension)' />
  </ItemGroup>
</Project>",
                "GetPackageContents", output);

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"build\netstandard2.0\Library.props",
            }));
        }

        [Fact]
        public void when_PackContent_false_but_content_has_Pack_then_packs()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Library</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
    <PackContent>false</PackContent>
  </PropertyGroup>
  <ItemGroup>
    <Content Include='Library.props' Pack='true' />
  </ItemGroup>
</Project>",
                "GetPackageContents", output);

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                Filename = "Library",
                Extension = ".props",
            }));
        }
    }
}
