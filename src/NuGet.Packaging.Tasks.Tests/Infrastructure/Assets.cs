using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace NuGet.Packaging.Tasks.Tests.Infrastructure
{
    public static class Assets
    {
        // TODO: simplify all this, since all files go to the test output directory.

        private static readonly string ProjectDirectory = ComputeProjectDirectory();

        public static string NuGetPackagingTargetsPath
        {
            get { return Path.Combine(ProjectDirectory, "src/NuGet.Packaging.Tasks"); }
        }

        public static string NuGetPackagingTasksPath
        {
            get
            {
                string baseDirectory = Path.GetDirectoryName(typeof(Assets).Assembly.Location);
                return Path.Combine(baseDirectory, "NuGet.Packaging.Tasks.dll");
            }
        }

        public static string NuGetToolPath
        {
            get { return Path.Combine(ProjectDirectory, "src/packages/NuGet.CommandLine.3.4.3/tools"); }
        }

        public static string NuGetExePath
        {
            get { return Path.Combine(NuGetToolPath, "nuget.exe"); }
        }

        public static string ScenariosDirectory
        {
            get { return Path.Combine(ProjectDirectory, "src", "NuGet.Packaging.Tasks.Tests", "Scenarios"); }
        }

        public static string GetScenarioDirectory([CallerMemberName] string scenarioName = null)
        {
            return Path.Combine(ScenariosDirectory, scenarioName);
        }

        public static string GetScenarioSolutionPath([CallerMemberName] string scenarioName = null)
        {
            var solutionDirectory = GetScenarioDirectory(scenarioName);
            return Path.Combine(solutionDirectory, scenarioName + ".sln");
        }

        public static string GetScenarioFilePath(string scenarioName, string filePath)
        {
            var solutionDirectory = GetScenarioDirectory(scenarioName);
            return Path.Combine(solutionDirectory, filePath);
        }

        private static string ComputeProjectDirectory()
        {
            var appDomainBase = Path.GetDirectoryName(typeof(Assets).Assembly.Location);

            // For IDE runs, the base directory will be something like
            //
            //      <ProjectDir>\src\NuProj.Tests\bin\Debug\
            //
            // When running from the automated build (command line or Visual Studio Online) we
            // run the tests from the output directory, which means the base directory will
            // look like this:
            //
            //      <ProjectDir>\bin\raw\
            //
            // This means we either have to go up 4 or 2 levels. In order to decide we simply
            // check whether the base directory is "raw"  -- which is a fixed part, even if
            // $(OutDir) is redirected.

            var isBuildOutput = string.Equals(Path.GetFileName(appDomainBase), "raw", StringComparison.OrdinalIgnoreCase);
            var parentPath = isBuildOutput ? @"../.." : @"../../../..";

            return Path.GetFullPath(Path.Combine(appDomainBase, parentPath));
        }
    }
}
