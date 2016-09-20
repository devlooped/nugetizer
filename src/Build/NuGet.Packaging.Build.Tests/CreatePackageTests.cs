using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NuGet.Packaging.Build.Tasks;
using Xunit;
using Xunit.Abstractions;
using System.Linq;
using static NuGet.Packaging.Build.Tasks.Properties.Strings;
using System.Diagnostics;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using Metadata = System.Collections.Generic.Dictionary<string, string>;

namespace NuGet.Packaging
{
	public class CreatePackageTests
	{
		ITestOutputHelper output;
		MockBuildEngine engine;
		CreatePackage task;
		bool createPackage;

		public CreatePackageTests(ITestOutputHelper output)
		{
			this.output = output;
			engine = new MockBuildEngine(output);
			task = new CreatePackage
			{
				BuildEngine = engine,

				Id = "package",
				Version = "1.0.0",
				Description = "description",
				Authors = "author1, author2",
			};

#if RELEASE
			// Create the actual .nupkg to ensure everything is working 
			// fine end to end.
			createPackage = true;
#endif

		}

		Manifest ExecuteTask() => createPackage ?
				task.Execute(new MemoryStream()) :
				task.CreateManifest();

		[Fact]
		public void when_creating_package_with_simple_dependency_then_contains_dependency_group()
		{
			task.Contents = new[]
			{
				new TaskItem("Newtonsoft.Json", new Metadata
				{
					{ MetadataName.PackageId, task.Id },
					{ MetadataName.Kind, PackageFileKind.Dependency },
					{ MetadataName.Version, "8.0.0" },
					// NOTE: AssignPackagePath takes care of converting TFM > short name
					{ MetadataName.TargetFramework, "net45" }
				}),
			};

			var manifest = ExecuteTask();

			Assert.NotNull(manifest);
			Assert.Equal(1, manifest.Metadata.DependencyGroups.Count());
			Assert.Equal(NuGetFramework.Parse(".NETFramework,Version=v4.5"), manifest.Metadata.DependencyGroups.First().TargetFramework);
			Assert.Equal(1, manifest.Metadata.DependencyGroups.First().Packages.Count());
			Assert.Equal("Newtonsoft.Json", manifest.Metadata.DependencyGroups.First().Packages.First().Id);

			// We get a version range actually for the specified dependency, like [1.0.0,)
			Assert.Equal("8.0.0", manifest.Metadata.DependencyGroups.First().Packages.First().VersionRange.MinVersion.ToString());
		}
	}
}
