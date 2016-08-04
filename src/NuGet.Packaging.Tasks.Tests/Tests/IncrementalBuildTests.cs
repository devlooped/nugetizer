using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using NuGet.Packaging.Tasks.Tests.Infrastructure;

using Xunit;

namespace NuGet.Packaging.Tasks.Tests
{
    public class IncrementalBuildTests : BuildProjectTests
    {
        [Fact]
        public async Task IncrementalBuild_NuSpecIsNotUpdated_WhenNothingChanged()
        {
            string sourceSolutionPath = Assets.GetScenarioSolutionPath("IncrementalBuild_NuSpecIsNotUpdated");
            string solutionPath = CopySolutionToTempDirectory(sourceSolutionPath);
            string projectPath = Path.Combine(Path.GetDirectoryName(solutionPath), "ClassLibrary");
            string nuspecFile = Path.Combine(projectPath, @"obj/Debug/_package.nuspec");

            // Perform first build
            await MSBuildRunner.BuildAsync(solutionPath, buildNuGet: true);

            // Get file stamp of the first nuspec file
            //
            // NOTE: We're asserting that the file exists because otherwise if the file doesn't
            //       exist FileInfo will simply return a placeholder value.

            var fileInfo1 = new FileInfo(nuspecFile);
            Assert.True(fileInfo1.Exists);

            var lastWriteTime1 = fileInfo1.LastWriteTimeUtc;

            // Wait for short period

            await Task.Delay(TimeSpan.FromMilliseconds(300));

            // Perform second build

            await MSBuildRunner.BuildAsync(solutionPath, buildNuGet: true);

            // Get file stamp of the nuspec file for the second build

            var fileInfo2 = new FileInfo(nuspecFile);
            Assert.True(fileInfo2.Exists);

            var lastWriteTime2 = fileInfo2.LastWriteTimeUtc;

            // The file stamps should match

            Assert.Equal(lastWriteTime1, lastWriteTime2);
        }

        [Fact]
        public async Task IncrementalBuild_NuSpecIsUpdated_WhenGlobalPropertiesChange()
        {
            string sourceSolutionPath = Assets.GetScenarioSolutionPath("IncrementalBuild_NuSpecIsUpdated");
            string solutionPath = CopySolutionToTempDirectory(sourceSolutionPath);
            string projectPath = Path.Combine(Path.GetDirectoryName(solutionPath), "ClassLibrary");
            string packagePath = Path.Combine(projectPath, @"bin/Debug/NuGetPackage.1.0.0.nupkg");

            const string expectedDescription1 = "First";
            const string expectedDescription2 = "Second";

            // Perform first build

            var properties = new Dictionary<string, string>();
            properties.Add("NuGetDescription", expectedDescription1);
            properties.Add("BuildNuGet", "True");
            await MSBuildRunner.BuildAsync(solutionPath, "Build", properties);

            string actualDescription1 = null;
            using (var reader = new PackageArchiveReader(File.OpenRead(packagePath))) {
                var nuspecReader = new NuspecReader(reader.GetNuspec());
                actualDescription1 = nuspecReader.GetMetadataValue("description");
            }

            // Perform second build

            properties["NuGetDescription"] = expectedDescription2;
            await MSBuildRunner.BuildAsync(solutionPath, "Build", properties);

            string actualDescription2 = null;
            using (var reader = new PackageArchiveReader(File.OpenRead(packagePath))) {
                var nuspecReader = new NuspecReader(reader.GetNuspec());
                actualDescription2 = nuspecReader.GetMetadataValue("description");
            }

            Assert.Equal(expectedDescription1, actualDescription1);
            Assert.Equal(expectedDescription2, actualDescription2);
        }
    }
}