
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NuGet.Packaging.Tasks.Tests.Infrastructure;
using Xunit;

namespace NuGet.Packaging.Tasks.Tests
{
    public class BuildNuGetTests : IDisposable
    {
        string tempRootDirectory;
        string tempSolutionDirectory;
        int directoryCount = 0;

        public BuildNuGetTests()
        {
            tempRootDirectory = Path.Combine(
                Path.GetDirectoryName(typeof(BuildNuGetTests).Assembly.Location),
                "test-projects");
        }

        public void Dispose()
        {
            Directory.Delete(tempRootDirectory, true);
        }

        string CopySolutionToTempDirectory(string sourceSolutionPath)
        {
            string sourceSolutionDirectory = Path.GetDirectoryName(sourceSolutionPath);
            string fileName = Path.GetFileName(sourceSolutionPath);

            tempSolutionDirectory = GetTempSolutionDirectory(sourceSolutionPath);

            CopyDirectory(sourceSolutionDirectory, tempSolutionDirectory);

            return Path.Combine(tempSolutionDirectory, fileName);
        }

        string GetTempSolutionDirectory (string sourceSolutionPath)
        {
            ++directoryCount;

            return Path.Combine(
                tempRootDirectory,
                Path.GetFileNameWithoutExtension(sourceSolutionPath) + "-" + directoryCount);
        }

        static void CopyDirectory(string source, string destination)
        {
            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            var sourceDirectoryInfo = new DirectoryInfo(source);

            FileInfo[] files = sourceDirectoryInfo.GetFiles();
            foreach (FileInfo file in files)
            {
                string fileDestinationPath = Path.Combine(destination, file.Name);
                file.CopyTo(fileDestinationPath, false);
            }

            foreach (DirectoryInfo subDirectoryInfo in sourceDirectoryInfo.GetDirectories())
            {
                string subDirectoryDestinationPath = Path.Combine(destination, subDirectoryInfo.Name);
                CopyDirectory(subDirectoryInfo.FullName, subDirectoryDestinationPath);
            }
        }

        [Fact]
        public async Task BuildNuGet_ClassLibraryWithNuGetDependency()
        {
            string sourceSolutionPath = Assets.GetScenarioSolutionPath("BuildNuGet_ClassLibraryWithNuGetDependency");
            string solutionPath = CopySolutionToTempDirectory(sourceSolutionPath);

            await NuGetRunner.RestorePackagesAsync(tempSolutionDirectory);

            await MSBuildRunner.RebuildAsync(solutionPath, buildNuGet: true);

            string packagePath = Path.Combine(tempSolutionDirectory, "bin", "Debug", "MyClassLibrary.0.1.2.nupkg");

            using (var reader = new PackageArchiveReader(File.OpenRead(packagePath))) {
                var nuspecReader = new NuspecReader(reader.GetNuspec());
                var identity = reader.GetIdentity();
                Assert.Equal("MyClassLibrary", identity.Id);
                Assert.Equal("0.1.2", identity.Version.ToString());
                Assert.Equal("MyClassLibrary", nuspecReader.GetId());
                Assert.Equal("0.1.2", nuspecReader.GetVersion().ToString());
                Assert.Equal("author1,author2", nuspecReader.GetMetadataValue("authors"));
                Assert.Equal("owner1,owner2", nuspecReader.GetMetadataValue("owners"));
                Assert.Equal("Desc", nuspecReader.GetMetadataValue("description"));
                Assert.Equal("false", nuspecReader.GetMetadataValue("requireLicenseAcceptance"));

                var packageDependencyGroup = reader.GetPackageDependencies().Single();
                Assert.Equal("net45", packageDependencyGroup.TargetFramework.GetShortFolderName());
                var packageDependency = packageDependencyGroup.Packages.Single();
                Assert.Equal("Newtonsoft.Json", packageDependency.Id);
                Assert.Equal("[9.0.1, )", packageDependency.VersionRange.ToString());

                var frameworkSpecificGroup = reader.GetLibItems().Single();
                Assert.Equal("net45", frameworkSpecificGroup.TargetFramework.GetShortFolderName());
                Assert.Contains("lib/net45/ClassLibraryWithNuGetDependency.xml", frameworkSpecificGroup.Items);
                Assert.Contains("lib/net45/ClassLibraryWithNuGetDependency.dll", frameworkSpecificGroup.Items);
                Assert.Equal(2, frameworkSpecificGroup.Items.Count());
            }
        }

        [Fact]
        public async Task BuildNuGet_ProjectReference()
        {
            string sourceSolutionPath = Assets.GetScenarioSolutionPath("BuildNuGet_ProjectReference");
            string solutionPath = CopySolutionToTempDirectory(sourceSolutionPath);

            await NuGetRunner.RestorePackagesAsync(tempSolutionDirectory);

            await MSBuildRunner.RebuildAsync(solutionPath, buildNuGet: true);

            string packagePath = Path.Combine(tempSolutionDirectory, "Main", "bin", "Debug", "Main.0.1.2.nupkg");

            using (var reader = new PackageArchiveReader(File.OpenRead(packagePath))) {
                var nuspecReader = new NuspecReader(reader.GetNuspec());
                var identity = reader.GetIdentity();
                Assert.Equal("Main", identity.Id);
                Assert.Equal("0.1.2", identity.Version.ToString());
                Assert.Equal("Main", nuspecReader.GetId());
                Assert.Equal("0.1.2", nuspecReader.GetVersion().ToString());
                Assert.Equal("author", nuspecReader.GetMetadataValue("authors"));
                Assert.Equal("owner", nuspecReader.GetMetadataValue("owners"));
                Assert.Equal("Desc", nuspecReader.GetMetadataValue("description"));
                Assert.Equal("false", nuspecReader.GetMetadataValue("requireLicenseAcceptance"));

                var packageDependencyGroup = reader.GetPackageDependencies().Single();
                Assert.Equal("net45", packageDependencyGroup.TargetFramework.GetShortFolderName());
                var packageDependency = packageDependencyGroup.Packages.Single(item => item.Id == "Newtonsoft.Json");
                Assert.Equal("[9.0.1, )", packageDependency.VersionRange.ToString());
                packageDependency = packageDependencyGroup.Packages.Single(item => item.Id == "Mono.Addins");
                Assert.Equal("[1.2.0, )", packageDependency.VersionRange.ToString());
                Assert.Equal(2, packageDependencyGroup.Packages.Count());

                var frameworkSpecificGroup = reader.GetLibItems().Single();
                Assert.Equal("net45", frameworkSpecificGroup.TargetFramework.GetShortFolderName());
                Assert.Contains("lib/net45/Main.dll", frameworkSpecificGroup.Items);
                Assert.Contains("lib/net45/Library.dll", frameworkSpecificGroup.Items);
                Assert.Equal(2, frameworkSpecificGroup.Items.Count());
            }
        }

        [Fact]
        public async Task BuildNuGet_ReferencedProjectBuildsPackage()
        {
            string sourceSolutionPath = Assets.GetScenarioSolutionPath("BuildNuGet_ReferencedProjectBuildsPackage");
            string solutionPath = CopySolutionToTempDirectory(sourceSolutionPath);

            await NuGetRunner.RestorePackagesAsync(tempSolutionDirectory);

            await MSBuildRunner.RebuildAsync(solutionPath, buildNuGet: true);

            string packagePath = Path.Combine(tempSolutionDirectory, "Main", "bin", "Debug", "Main.0.1.2.nupkg");

            using (var reader = new PackageArchiveReader(File.OpenRead(packagePath))) {
                var identity = reader.GetIdentity();
                Assert.Equal("Main", identity.Id);

                var packageDependencyGroup = reader.GetPackageDependencies().Single();
                Assert.Equal("net45", packageDependencyGroup.TargetFramework.GetShortFolderName());
                var packageDependency = packageDependencyGroup.Packages.FirstOrDefault(item => item.Id == "Newtonsoft.Json");
                Assert.Equal("[9.0.1, )", packageDependency.VersionRange.ToString());
                packageDependency = packageDependencyGroup.Packages.FirstOrDefault(item => item.Id == "Library");
                Assert.Equal("[1.2.0, )", packageDependency.VersionRange.ToString());
                Assert.Equal(2, packageDependencyGroup.Packages.Count());

                var frameworkSpecificGroup = reader.GetLibItems().Single();
                Assert.Equal("net45", frameworkSpecificGroup.TargetFramework.GetShortFolderName());
                Assert.Contains("lib/net45/Main.dll", frameworkSpecificGroup.Items);
                Assert.Equal(1, frameworkSpecificGroup.Items.Count());
            }

            packagePath = Path.Combine(tempSolutionDirectory, "Library", "bin", "Debug", "Library.1.2.0.nupkg");

            using (var reader = new PackageArchiveReader(File.OpenRead(packagePath))) {
                var nuspecReader = new NuspecReader(reader.GetNuspec());
                var identity = reader.GetIdentity();
                Assert.Equal("Library", identity.Id);
                Assert.Equal("1.2.0", identity.Version.ToString());
                Assert.Equal("Library", nuspecReader.GetId());
                Assert.Equal("1.2.0", nuspecReader.GetVersion().ToString());
                Assert.Equal("author", nuspecReader.GetMetadataValue("authors"));
                Assert.Equal("owner", nuspecReader.GetMetadataValue("owners"));
                Assert.Equal("Desc", nuspecReader.GetMetadataValue("description"));
                Assert.Equal("false", nuspecReader.GetMetadataValue("requireLicenseAcceptance"));

                var packageDependencyGroup = reader.GetPackageDependencies().Single();
                Assert.Equal("net45", packageDependencyGroup.TargetFramework.GetShortFolderName());
                var packageDependency = packageDependencyGroup.Packages.Single(item => item.Id == "Mono.Addins");
                Assert.Equal("[1.2.0, )", packageDependency.VersionRange.ToString());
                Assert.Equal(1, packageDependencyGroup.Packages.Count());

                var frameworkSpecificGroup = reader.GetLibItems().Single();
                Assert.Equal("net45", frameworkSpecificGroup.TargetFramework.GetShortFolderName());
                Assert.Contains("lib/net45/Library.dll", frameworkSpecificGroup.Items);
                Assert.Equal(1, frameworkSpecificGroup.Items.Count());
            }
        }

        [Fact]
        public async Task BuildNuGet_ReferenceOutputAssemblyIsFalse()
        {
            string sourceSolutionPath = Assets.GetScenarioSolutionPath("BuildNuGet_ReferenceOutputAssemblyIsFalse");
            string solutionPath = CopySolutionToTempDirectory(sourceSolutionPath);

            await NuGetRunner.RestorePackagesAsync(tempSolutionDirectory);

            await MSBuildRunner.RebuildAsync(solutionPath, buildNuGet: true);

            string packagePath = Path.Combine(tempSolutionDirectory, "Main", "bin", "Debug", "Main.0.1.2.nupkg");

            using (var reader = new PackageArchiveReader(File.OpenRead(packagePath))) {
                var identity = reader.GetIdentity();
                Assert.Equal("Main", identity.Id);
                Assert.Equal("0.1.2", identity.Version.ToString());

                var packageDependencyGroup = reader.GetPackageDependencies().Single();
                Assert.Equal("net45", packageDependencyGroup.TargetFramework.GetShortFolderName());
                var packageDependency = packageDependencyGroup.Packages.Single(item => item.Id == "Newtonsoft.Json");
                Assert.Equal("[9.0.1, )", packageDependency.VersionRange.ToString());
                Assert.Equal(1, packageDependencyGroup.Packages.Count());

                var frameworkSpecificGroup = reader.GetLibItems().Single();
                Assert.Equal("net45", frameworkSpecificGroup.TargetFramework.GetShortFolderName());
                Assert.Contains("lib/net45/Main.dll", frameworkSpecificGroup.Items);
                Assert.Equal(1, frameworkSpecificGroup.Items.Count());
            }
        }
    }
}

