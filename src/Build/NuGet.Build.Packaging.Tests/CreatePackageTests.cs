using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Xunit;
using Xunit.Abstractions;
using System.Linq;
using static NuGet.Build.Packaging.Properties.Strings;
using System.Diagnostics;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using Metadata = System.Collections.Generic.Dictionary<string, string>;
using NuGet.Build.Packaging.Tasks;
using NuGet.Packaging;

namespace NuGet.Build.Packaging
{
	public class CreatePackageTests
	{
		ITestOutputHelper output;
		MockBuildEngine engine;
		CreatePackage task;
		bool createPackage = false;

		public CreatePackageTests(ITestOutputHelper output)
		{
			this.output = output;
			engine = new MockBuildEngine(output);
			task = new CreatePackage
			{
				BuildEngine = engine,
				Manifest = new TaskItem("package", new Metadata
				{
					{ "Id", "package" },
					{ "Version", "1.0.0" },
					{ "Title", "title" },
					{ "Description", "description" },
					{ "Summary", "summary" },
					{ "Language", "en" },

					{ "Copyright", "copyright" },
					{ "RequireLicenseAcceptance", "true" },

					{ "Authors", "author1, author2" },
					{ "Owners", "owner1, owner2" },
					{ "Tags", "nuget msbuild" },

					{ "LicenseUrl", "http://contoso.com/license.txt" },
					{ "ProjectUrl", "http://contoso.com/" },
					{ "IconUrl", "http://contoso.com/icon.png" },
					{ "ReleaseNotes", "release notes" },
					{ "MinClientVersion", "3.4.0" },
				})
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
		public void when_creating_package_then_contains_all_metadata()
		{
			task.Contents = new[]
			{
				// Need at least one dependency or content file for the generation to succeed.
				new TaskItem("Newtonsoft.Json", new Metadata
				{
					{ MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
					{ MetadataName.Kind, PackageItemKind.Dependency },
					{ MetadataName.Version, "8.0.0" },
					{ MetadataName.TargetFramework, "net45" }
				}),
			};

			var metadata = ExecuteTask().Metadata;

			Assert.True(metadata.RequireLicenseAcceptance);

			Assert.Equal(task.Manifest.GetMetadata("Id"), metadata.Id);
			Assert.Equal(task.Manifest.GetMetadata("Version"), metadata.Version.ToString());
			Assert.Equal(task.Manifest.GetMetadata("Title"), metadata.Title);
			Assert.Equal(task.Manifest.GetMetadata("Description"), metadata.Description);
			Assert.Equal(task.Manifest.GetMetadata("Summary"), metadata.Summary);
			Assert.Equal(task.Manifest.GetMetadata("Language"), metadata.Language);
			Assert.Equal(task.Manifest.GetMetadata("Copyright"), metadata.Copyright);
			Assert.Equal(task.Manifest.GetMetadata("LicenseUrl"), metadata.LicenseUrl.ToString());
			Assert.Equal(task.Manifest.GetMetadata("ProjectUrl"), metadata.ProjectUrl.ToString());
			Assert.Equal(task.Manifest.GetMetadata("IconUrl"), metadata.IconUrl.ToString());
			Assert.Equal(task.Manifest.GetMetadata("ReleaseNotes"), metadata.ReleaseNotes);
			Assert.Equal(task.Manifest.GetMetadata("MinClientVersion"), metadata.MinClientVersion.ToString());
		}

		[Fact]
		public void when_creating_package_has_development_dependency_metadata_then_manifest_has_development_dependency()
		{
			task.Contents = new[]
			{
				// Need at least one dependency or content file for the generation to succeed.
				new TaskItem("Newtonsoft.Json", new Metadata
				{
					{ MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
					{ MetadataName.Kind, PackageItemKind.Dependency },
					{ MetadataName.Version, "8.0.0" },
					{ MetadataName.TargetFramework, "net45" }
				}),
			};

			task.Manifest.SetMetadata("DevelopmentDependency", "true");

			var metadata = ExecuteTask().Metadata;

			Assert.True(metadata.DevelopmentDependency);
		}

		[Fact]
		public void when_creating_package_with_simple_dependency_then_contains_dependency_group()
		{
			task.Contents = new[]
			{
				new TaskItem("Newtonsoft.Json", new Metadata
				{
					{ MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
					{ MetadataName.Kind, PackageItemKind.Dependency },
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

		[Fact]
		public void when_creating_package_with_development_dependency_then_does_not_generate_dependency_group()
		{
			var content = Path.GetTempFileName();
			task.Contents = new[]
			{
				new TaskItem(content, new Metadata
				{
					{ MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
					{ MetadataName.Kind, PackageItemKind.None },
					{ MetadataName.PackagePath, "readme.txt" }
				}),
				new TaskItem("Helpers", new Metadata
				{
					{ MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
					{ MetadataName.Version, "1.0.0" },
					{ MetadataName.Kind, PackageItemKind.Dependency },
					{ MetadataName.IsDevelopmentDependency, "true" },
					// NOTE: AssignPackagePath takes care of converting TFM > short name
					{ MetadataName.TargetFramework, "net45" }
				}),
			};

			var manifest = ExecuteTask();

			Assert.Equal(0, manifest.Metadata.DependencyGroups.Count());
		}

		[Fact]
		public void when_creating_package_with_referenced_package_project_then_contains_package_dependency()
		{
			task.Contents = new[]
			{
				new TaskItem("Newtonsoft.Json", new Metadata
				{
					{ MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
					{ MetadataName.Kind, PackageItemKind.Dependency },
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

		[Fact]
		public void when_creating_package_with_file_then_contains_file()
		{
			var content = Path.GetTempFileName();
			task.Contents = new[]
			{
				new TaskItem(content, new Metadata
				{
					{ MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
					{ MetadataName.Kind, PackageItemKind.None },
					{ MetadataName.PackagePath, "readme.txt" }
				}),
			};

			var manifest = ExecuteTask();

			Assert.NotNull(manifest);
			Assert.Contains(manifest.Files, file => file.Target == "readme.txt");
		}

		[Fact]
		public void when_creating_package_with_content_file_then_adds_as_content_file()
		{
			var content = Path.GetTempFileName();
			task.Contents = new[]
			{
				new TaskItem(content, new Metadata
				{
					{ MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
					{ MetadataName.Kind, PackageItemKind.ContentFiles },
					{ MetadataName.PackageFolder, PackagingConstants.Folders.ContentFiles },
					{ MetadataName.PackagePath, @"contentFiles\any\any\readme.txt" }
				}),
			};

			var manifest = ExecuteTask();

			Assert.Contains(manifest.Files, file => file.Target == @"contentFiles\any\any\readme.txt");
			Assert.Contains(manifest.Metadata.ContentFiles, file => file.Include == @"contentFiles\any\any\readme.txt");
		}

		[Fact]
		public void when_creating_package_with_content_file_build_action_then_adds_as_content_file()
		{
			var content = Path.GetTempFileName();
			task.Contents = new[]
			{
				new TaskItem(content, new Metadata
				{
					{ MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
					{ MetadataName.Kind, PackageItemKind.ContentFiles },
					{ MetadataName.PackageFolder, PackagingConstants.Folders.ContentFiles },
					{ MetadataName.ContentFile.BuildAction, "EmbeddedResource" },
					{ MetadataName.PackagePath, @"contentFiles\any\any\readme.txt" }
				}),
			};

			var manifest = ExecuteTask();

			Assert.Contains(manifest.Files, file => file.Target == @"contentFiles\any\any\readme.txt");
			Assert.Contains(manifest.Metadata.ContentFiles, file => file.Include == @"contentFiles\any\any\readme.txt" && file.BuildAction == "EmbeddedResource");
		}
		
		[Fact]
		public void when_creating_package_with_content_file_copy_to_output_then_adds_as_content_file()
		{
			var none = Path.GetTempFileName();
			var content = Path.GetTempFileName();
			task.Contents = new[]
			{
				new TaskItem(content, new Metadata
				{
					{ MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
					{ MetadataName.Kind, PackageItemKind.ContentFiles },
					{ MetadataName.PackageFolder, PackagingConstants.Folders.ContentFiles },
					{ MetadataName.ContentFile.CopyToOutput, "true" },
					{ MetadataName.PackagePath, @"contentFiles\any\any\readme.txt" }
				}),
			};

			var manifest = ExecuteTask();

			Assert.Equal(new[]
				{
					new ManifestFile { Target = @"contentFiles\any\any\readme.txt" }
				},
				manifest.Files, ManifestFileComparer.Default);

			Assert.Equal(new[]
				{
					new ManifestContentFiles { Include = @"contentFiles\any\any\readme.txt", CopyToOutput = "true" }
				},
				manifest.Metadata.ContentFiles, ManifestContentFilesComparer.Default);
		}

		[Fact]
		public void when_creating_package_with_content_file_flatten_then_adds_as_content_file()
		{
			var none = Path.GetTempFileName();
			var content = Path.GetTempFileName();
			task.Contents = new[]
			{
				new TaskItem(content, new Metadata
				{
					{ MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
					{ MetadataName.Kind, PackageItemKind.ContentFiles },
					{ MetadataName.PackageFolder, PackagingConstants.Folders.ContentFiles },
					{ MetadataName.ContentFile.Flatten, "true" },
					{ MetadataName.PackagePath, @"contentFiles\any\any\readme.txt" }
				}),
			};

			var manifest = ExecuteTask();

			Assert.Equal(new[]
				{
					new ManifestFile
					{
						Target = @"contentFiles\any\any\readme.txt"
					}
				},
				manifest.Files,ManifestFileComparer.Default);

			Assert.Equal(new[]
				{
					new ManifestContentFiles
					{
						Include = @"contentFiles\any\any\readme.txt",
						Flatten = "true"
					}
				},
				manifest.Metadata.ContentFiles, ManifestContentFilesComparer.Default);
		}

		[Fact]
		public void when_creating_package_with_framework_reference_then_contains_references()
		{
			task.Contents = new[]
			{
				new TaskItem("System.Xml", new Metadata
				{
					{ MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
					{ MetadataName.Kind, PackageItemKind.FrameworkReference },
					{ MetadataName.TargetFrameworkMoniker, TargetFrameworks.NET45 }
				}),
				new TaskItem("System.Xml", new Metadata
				{
					{ MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
					{ MetadataName.Kind, PackageItemKind.FrameworkReference },
					{ MetadataName.TargetFrameworkMoniker, TargetFrameworks.PCL78 }
				}),
			};

			var manifest = ExecuteTask();

			Assert.Equal(manifest.Metadata.FrameworkReferences,
				new[]
				{
					new FrameworkAssemblyReference("System.Xml", new [] { NuGetFramework.Parse(TargetFrameworks.NET45) }),
					new FrameworkAssemblyReference("System.Xml", new [] { NuGetFramework.Parse(TargetFrameworks.PCL78) }),
				}, 
				FrameworkAssemblyReferenceComparer.Default);

			Assert.NotNull(manifest);
		}
	}
}
