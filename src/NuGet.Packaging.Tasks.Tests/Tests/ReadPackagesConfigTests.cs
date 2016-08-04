using System.Linq;
using Microsoft.Build.Utilities;
using NuGet.Packaging.Tasks.Tests.Infrastructure;
using NuGet.Versioning;
using Xunit;

namespace NuGet.Packaging.Tasks.Tests
{
    public class ReadPackagesConfigTests
    {
        [Fact]
        public void ReadPackagesConfig_ParseProjectoryConfig()
        {
            var projectPath = Assets.GetScenarioFilePath("ReadPackagesConfig_ParseProjectoryConfig", @"A.csproj");

            var task = new ReadPackagesConfig();
            task.Projects = new[] { new TaskItem(projectPath) };
            var result = task.Execute();

            var expectedVersionConstraint = VersionRange.Parse("[1,2]").ToString();

            Assert.True(result);
            var fodyItem = new AssertTaskItem(task.PackageReferences, "Fody", items => Assert.Single(items)) {
                {"Version", "1.25.0"},
                {"TargetFramework", "net45"},
                {"VersionConstraint", expectedVersionConstraint},
                {"RequireReinstallation", bool.FalseString},
                {"IsDevelopmentDependency", bool.TrueString},
            };

            Assert.Single(fodyItem);
        }

        [Fact]
        public void ReadPackagesConfig_ParseDirectoryConfig()
        {
            var projectPath = Assets.GetScenarioFilePath("ReadPackagesConfig_ParseDirectoryConfig", @"B.csproj");

            var task = new ReadPackagesConfig();
            task.Projects = new[] { new TaskItem(projectPath) };
            var result = task.Execute();

            Assert.True(result);

            Assert.Single(task.PackageReferences.Where(x => x.ItemSpec == "Microsoft.Bcl.Immutable"));
            Assert.Single(task.PackageReferences.Where(x => x.ItemSpec == "Microsoft.Bcl.Metadata"));
            Assert.Single(task.PackageReferences.Where(x => x.ItemSpec == "Microsoft.CodeAnalysis.Common"));
            Assert.Single(task.PackageReferences.Where(x => x.ItemSpec == "Microsoft.CodeAnalysis.CSharp"));
        }

        [Fact]
        public void ReadPackagesConfig_NoConfig()
        {
            var projectPath = Assets.GetScenarioFilePath("ReadPackagesConfig_NoConfig", @"C.csproj");

            var task = new ReadPackagesConfig();
            task.Projects = new[] { new TaskItem(projectPath) };
            var result = task.Execute();

            Assert.True(result);
            Assert.Empty(task.PackageReferences);
        }
    }
}