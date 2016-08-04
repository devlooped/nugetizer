using System.Linq;
using Microsoft.Build.Utilities;
using NuGet.Packaging.Tasks.Tests.Infrastructure;
using NuGet.Versioning;
using Xunit;

namespace NuGet.Packaging.Tasks.Tests
{
    public class ReadProjectJsonTests
    {
        [Fact]
        public void ReadProjectJson_SpecificPackageVersion()
        {
            var projectPath = Assets.GetScenarioFilePath("ReadProjectJson_SpecificPackageVersion", @"A.csproj");

            var task = new ReadProjectJson();
            task.TargetFrameworkMoniker = ".NETFramework, Version=4.5";
            task.Projects = new[] { new TaskItem(projectPath) };
            var result = task.Execute();

            Assert.True(result);

            Assert.Single(task.PackageReferences.Where(x => x.ItemSpec == "Microsoft.Bcl.Immutable"));
            Assert.Single(task.PackageReferences.Where(x => x.ItemSpec == "Microsoft.Bcl.Metadata"));
            Assert.Single(task.PackageReferences.Where(x => x.ItemSpec == "Microsoft.CodeAnalysis.Common"));
            Assert.Single(task.PackageReferences.Where(x => x.ItemSpec == "Microsoft.CodeAnalysis.CSharp"));
            var packageReferenceItem = task.PackageReferences.Single(x => x.ItemSpec == "Microsoft.Bcl.Immutable");
            Assert.Equal("1.1.20-beta", packageReferenceItem.GetMetadata(Metadata.Version));
            Assert.Equal("net45", packageReferenceItem.GetMetadata(Metadata.TargetFramework));
        }
    }
}