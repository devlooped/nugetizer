using Xunit;
using Xunit.Abstractions;

namespace NuGetizer
{
    public class given_sourcelink
    {
        ITestOutputHelper output;

        public given_sourcelink(ITestOutputHelper output) => this.output = output;

        [Fact]
        public void when_getting_metadata_then_adds_repository_info()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Library</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
    <PublishRepositoryUrl>true</PublishRepositoryUrl>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Microsoft.SourceLink.GitHub' Version='1.0.0' PrivateAssets='all' />
  </ItemGroup>
</Project>",
            "GetPackageMetadata", output);

            result.AssertSuccess(output);

            Assert.Single(result.Items);
            var metadata = result.Items[0];

            Assert.Equal("git", metadata.GetMetadata("RepositoryType"));
            Assert.Equal("https://github.com/kzu/NuGetizer", metadata.GetMetadata("RepositoryUrl"));
            Assert.NotEmpty(metadata.GetMetadata("RepositoryCommit"));
        }

        [Fact]
        public void when_getting_metadata_with_no_explicit_publish_repo_url_then_does_not_expose_it()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Library</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Microsoft.SourceLink.GitHub' Version='1.0.0' PrivateAssets='all' />
  </ItemGroup>
</Project>",
            "GetPackageMetadata", output);

            result.AssertSuccess(output);

            Assert.Single(result.Items);
            var metadata = result.Items[0];

            Assert.Empty(metadata.GetMetadata("RepositoryUrl"));
            Assert.NotEmpty(metadata.GetMetadata("RepositoryCommit"));
        }
    }
}
