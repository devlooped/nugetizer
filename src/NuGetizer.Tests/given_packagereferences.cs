using Microsoft.Build.Execution;
using Xunit;
using Xunit.Abstractions;

namespace NuGetizer
{
    public class given_packagereferences
    {
        ITestOutputHelper output;

        public given_packagereferences(ITestOutputHelper output) => this.output = output;

        [Fact]
        public void when_privateassets_all_then_packs_library()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Library</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Newtonsoft.Json' Version='1.0.0' PrivateAssets='all' />
  </ItemGroup>
</Project>",
                "GetPackageContents", output);

            result.AssertSuccess(output);
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackagePath = @"lib\netstandard2.0\Newtonsoft.Json.dll",
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
                PackagePath = @"lib\netstandard2.0\Newtonsoft.Json.dll",
            }));
        }
    }
}
