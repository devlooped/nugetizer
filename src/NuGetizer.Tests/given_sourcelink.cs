using Microsoft.Build.Execution;
using Xunit;
using Xunit.Abstractions;

namespace NuGetizer
{
    public class given_sourcelink
	{
		ITestOutputHelper output;

		public given_sourcelink(ITestOutputHelper output)
		{
			this.output = output;
            Builder.BuildScenario(nameof(given_sourcelink), target: "Restore")
                .AssertSuccess(output);
        }

        [Fact]
        public void when_getting_metadata_then_adds_repository_info()
        {
            var result = Builder.BuildScenario(nameof(given_an_empty_library), new 
            { 
                PackageId = "Foo",
                PublishRepositoryUrl = "true",
            }, target: "GetPackageMetadata", output: output);

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
            var result = Builder.BuildScenario(nameof(given_an_empty_library), new
            {
                PackageId = "Foo",
            }, target: "GetPackageMetadata", output: output);

            result.AssertSuccess(output);

            Assert.Single(result.Items);
            var metadata = result.Items[0];

            Assert.Empty(metadata.GetMetadata("RepositoryUrl"));
            Assert.NotEmpty(metadata.GetMetadata("RepositoryCommit"));
        }
    }
}
