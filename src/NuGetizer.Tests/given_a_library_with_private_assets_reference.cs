using NuGetizer.Tests;
using Xunit;
using Xunit.Abstractions;

namespace NuGetizer
{
    public class given_a_library_with_private_assets_reference
    {
        readonly ITestOutputHelper output;

        public given_a_library_with_private_assets_reference(ITestOutputHelper output) => this.output = output;

        [RuntimeFact("Windows")]
        public void when_getting_package_contents_then_contains_private_assets_as_primary_output()
        {
            using (var disable = OpenBuildLogAttribute.Disable())
                Builder.BuildScenario(nameof(given_a_library_with_private_assets_reference), target: "Restore", output: output)
                    .AssertSuccess(output);

            var result = Builder.BuildScenario(nameof(given_a_library_with_private_assets_reference), output: output);

            result.AssertSuccess(output);

            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                PackFolder = PackFolderKind.Dependency,
                Identity = "Mono.Options",
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackFolder = PackFolderKind.Lib,
                Filename = "Mono.Options",
                NuGetPackageId = "Mono.Options",
            }));
        }

        [RuntimeFact("Windows")]
        public void when_getting_package_contents_then_contains_private_lib_assets_as_primary_output_and_also_package_reference()
        {
            Builder.BuildScenario(nameof(given_a_library_with_private_assets_reference), target: "Restore", output: output)
                .AssertSuccess(output);

            var result = Builder.BuildScenario(nameof(given_a_library_with_private_assets_reference), output: output);

            result.AssertSuccess(output);

            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackFolder = PackFolderKind.Dependency,
                Identity = "Newtonsoft.Json",
            }));
            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackFolder = PackFolderKind.Lib,
                Filename = "Newtonsoft.Json",
                NuGetPackageId = "Newtonsoft.Json",
            }));
        }

        [RuntimeFact("Windows")]
        public void when_getting_package_contents_then_contains_dependency_for_non_private_assets_reference()
        {
            Builder.BuildScenario(nameof(given_a_library_with_private_assets_reference), target: "Restore", output: output)
                .AssertSuccess(output);

            var result = Builder.BuildScenario(nameof(given_a_library_with_private_assets_reference), output: output);

            result.AssertSuccess(output);

            Assert.Contains(result.Items, item => item.Matches(new
            {
                PackFolder = PackFolderKind.Dependency,
                Identity = "xunit",
            }));
            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                PackFolder = PackFolderKind.Lib,
                Filename = "xunit",
                NuGetPackageId = "xunit",
            }));
        }
    }
}
