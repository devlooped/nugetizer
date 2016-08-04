using System;
using System.IO;

namespace NuGet.Packaging.Tasks.Tests
{
    public abstract class BuildProjectTests : IDisposable
    {
        protected string tempRootDirectory;
        protected string tempSolutionDirectory;
        int directoryCount = 0;

        public BuildProjectTests()
        {
            tempRootDirectory = Path.Combine(
                Path.GetDirectoryName(typeof(BuildProjectTests).Assembly.Location),
                "test-projects");
        }

        public void Dispose()
        {
            Directory.Delete(tempRootDirectory, true);
        }

        protected string CopySolutionToTempDirectory(string sourceSolutionPath)
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
    }
}

