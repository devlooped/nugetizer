using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace NuGetizer
{
    public class given_a_library_with_project_reference
    {
        readonly ITestOutputHelper output;

        public given_a_library_with_project_reference(ITestOutputHelper output)
        {
            this.output = output;
            Builder.BuildScenario(nameof(given_a_library_with_project_reference), output: output, target: "Restore")
                .AssertSuccess(output);
        }

        [Fact]
        public void when_getting_package_contents_then_retrieves_main_assembly_transitively()
        {
            var result = Builder.BuildScenario(nameof(given_a_library_with_project_reference), output: output);

            result.AssertSuccess(output);

            // TODO: build some helpers to make this easier to assert.
            Assert.True(result.Items.Any(i => i.GetMetadata("FileName") == "a" && i.GetMetadata("Extension") == ".dll" && i.GetMetadata("PackFolder") == PackFolderKind.Lib), "Did not include main project output as Library");
            Assert.True(result.Items.Any(i => i.GetMetadata("FileName") == "b" && i.GetMetadata("Extension") == ".dll" && i.GetMetadata("PackFolder") == PackFolderKind.Lib), "Did not include referenced project output as Library");
        }

        [Fact]
        public void when_getting_package_contents_then_retrieves_symbols_transitively()
        {
            var result = Builder.BuildScenario(nameof(given_a_library_with_project_reference), output: output);

            result.AssertSuccess(output);

            // TODO: build some helpers to make this easier to assert.
            Assert.True(result.Items.Any(i => i.GetMetadata("FileName") == "a" && i.GetMetadata("Extension") == ".pdb" && i.GetMetadata("PackFolder") == PackFolderKind.Lib), "Did not include main project symbols");
            Assert.True(result.Items.Any(i => i.GetMetadata("FileName") == "b" && i.GetMetadata("Extension") == ".pdb" && i.GetMetadata("PackFolder") == PackFolderKind.Lib), "Did not include referenced project symbols");
        }

        [Fact]
        public void when_include_in_package_false_then_does_not_include_referenced_project_outputs()
        {
            var result = Builder.BuildScenario(nameof(given_a_library_with_project_reference),
                properties: new { IncludeInPackage = "false" },
                output: output);

            result.AssertSuccess(output);

            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                Filename = "b",
                Extension = ".dll",
                PackFolder = "lib",
            }));
        }

        [Fact]
        public void when_include_outputs_in_package_false_then_can_include_referenced_project_outputs()
        {
            var result = Builder.BuildScenario(nameof(given_a_library_with_project_reference),
                properties: new
                {
                    PackBuildOutput = "false",
                    PackProjectReference = "true"
                },
                output: output);

            result.AssertSuccess(output);

            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                Filename = "b",
                Extension = ".dll",
                PackFolder = "lib",
            }));
        }

        [Fact]
        public void when_include_outputs_in_package_false_then_can_include_referenced_projects_outputs()
        {
            var result = Builder.BuildScenario(nameof(given_a_library_with_project_reference),
                properties: new
                {
                    PackBuildOutput = "false",
                    PackProjectReferences = "true"
                },
                output: output);

            result.AssertSuccess(output);

            Assert.DoesNotContain(result.Items, item => item.Matches(new
            {
                Filename = "b",
                Extension = ".dll",
                PackFolder = "lib",
            }));
        }

        [Fact(Skip = "Doesn't work running from test, but does from CLI :(")]
        public void when_pack_no_build_then_does_not_fail()
        {
            // Build once, to simulate a previous step in a CI pipeline.
            Builder.BuildScenario(nameof(given_a_library_with_project_reference),
                target: "Build",
                properties: new { MainPackageId = "Library" },
                output: output).AssertSuccess(output);

            // Now pack without building
            var result = Builder.BuildScenario(nameof(given_a_library_with_project_reference),
                target: "Pack",
                properties: new { MainPackageId = "Library", NoBuild = "true" },
                output: output);

            result.AssertSuccess(output);
        }
    }
}
