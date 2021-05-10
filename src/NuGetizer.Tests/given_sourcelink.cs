using System;
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
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_REF")))
                Environment.SetEnvironmentVariable("GITHUB_REF", "refs/heads/main");

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
            Assert.Equal(ThisAssembly.Project.PrivateRepositoryUrl, metadata.GetMetadata("RepositoryUrl"));
            Assert.NotEmpty(metadata.GetMetadata("RepositoryCommit"));
            Assert.NotEmpty(metadata.GetMetadata("RepositoryBranch"));
        }

        [Fact]
        public void when_no_project_url_and_publish_repository_url_then_defaults_to_repository_url()
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

            Assert.Equal(ThisAssembly.Project.PrivateRepositoryUrl, metadata.GetMetadata("ProjectUrl"));
        }

        [Fact]
        public void when_project_url_and_publish_repository_url_then_preserves_value()
        {
            var result = Builder.BuildProject(@"
<Project Sdk='Microsoft.NET.Sdk'>
  <PropertyGroup>
    <PackageId>Library</PackageId>
    <TargetFramework>netstandard2.0</TargetFramework>
    <PublishRepositoryUrl>true</PublishRepositoryUrl>
    <PackageProjectUrl>foo.com</PackageProjectUrl>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include='Microsoft.SourceLink.GitHub' Version='1.0.0' PrivateAssets='all' />
  </ItemGroup>
</Project>",
            "GetPackageMetadata", output);

            result.AssertSuccess(output);

            Assert.Single(result.Items);
            var metadata = result.Items[0];

            Assert.Equal("foo.com", metadata.GetMetadata("ProjectUrl"));
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
