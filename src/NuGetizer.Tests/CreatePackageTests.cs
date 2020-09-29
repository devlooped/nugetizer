using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Xunit;
using Xunit.Abstractions;
using System.Linq;
using System.Diagnostics;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using Metadata = System.Collections.Generic.Dictionary<string, string>;
using NuGetizer.Tasks;
using NuGet.Packaging;
using NuGet.Packaging.Licenses;

namespace NuGetizer
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
				NuspecFile = Path.GetTempFileName(),
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
					{ "PackageTypes", PackageType.Dependency.Name }
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

			Assert.Equal(task.Manifest.GetMetadata("PackageId"), metadata.Id);
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
			Assert.Collection(metadata.PackageTypes,
				item =>
				{
					Assert.Equal(PackageType.Dependency.Name, item.Name);
					Assert.Equal(PackageType.EmptyVersion, item.Version);
				});
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
        public void when_creating_package_has_license_expression_then_manifest_has_license()
        {
            task.Manifest.SetMetadata("LicenseUrl", "");
            task.Manifest.SetMetadata("LicenseExpression", "MIT");

            var metadata = ExecuteTask().Metadata;

            Assert.Equal("MIT", metadata.LicenseMetadata.License);
            Assert.Equal(LicenseType.Expression, metadata.LicenseMetadata.Type);
            Assert.Equal(LicenseExpressionType.License, metadata.LicenseMetadata.LicenseExpression.Type);
            Assert.True(metadata.LicenseMetadata.LicenseExpression is NuGetLicense license && license.IsStandardLicense);
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
			Assert.Single(manifest.Metadata.DependencyGroups);
			Assert.Equal(NuGetFramework.Parse(".NETFramework,Version=v4.5"), manifest.Metadata.DependencyGroups.First().TargetFramework);
			Assert.Single(manifest.Metadata.DependencyGroups.First().Packages);
			Assert.Equal("Newtonsoft.Json", manifest.Metadata.DependencyGroups.First().Packages.First().Id);

			// We get a version range actually for the specified dependency, like [1.0.0,)
			Assert.Equal("8.0.0", manifest.Metadata.DependencyGroups.First().Packages.First().VersionRange.MinVersion.ToString());
		}

		[Fact]
		public void when_creating_package_with_dependency_and_include_assets_then_contains_dependency_include_attribute()
		{
			task.Contents = new[]
			{
				new TaskItem("Newtonsoft.Json", new Metadata
				{
					{ MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
					{ MetadataName.Kind, PackageItemKind.Dependency },
					{ MetadataName.Version, "8.0.0" },
					// NOTE: AssignPackagePath takes care of converting TFM > short name
					{ MetadataName.TargetFramework, "net45" },
					{ MetadataName.IncludeAssets, "all" }
				}),
			};

			var manifest = ExecuteTask();

			Assert.NotNull(manifest);
			Assert.Single(manifest.Metadata.DependencyGroups);
			Assert.Single(manifest.Metadata.DependencyGroups.First().Packages);
			Assert.Equal("Newtonsoft.Json", manifest.Metadata.DependencyGroups.First().Packages.First().Id);
			Assert.Equal(1, manifest.Metadata.DependencyGroups.First().Packages.First().Include.Count);
			Assert.Equal("all", manifest.Metadata.DependencyGroups.First().Packages.First().Include[0]);
		}

		[Fact]
		public void when_creating_package_with_dependency_and_exclude_assets_then_contains_dependency_exclude_attribute()
		{
			task.Contents = new[]
			{
				new TaskItem("Newtonsoft.Json", new Metadata
				{
					{ MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
					{ MetadataName.Kind, PackageItemKind.Dependency },
					{ MetadataName.Version, "8.0.0" },
					// NOTE: AssignPackagePath takes care of converting TFM > short name
					{ MetadataName.TargetFramework, "net45" },
					{ MetadataName.ExcludeAssets, "all" }
				}),
			};

			var manifest = ExecuteTask();

			Assert.NotNull(manifest);
			Assert.Single(manifest.Metadata.DependencyGroups);
			Assert.Single(manifest.Metadata.DependencyGroups.First().Packages);
			Assert.Equal("Newtonsoft.Json", manifest.Metadata.DependencyGroups.First().Packages.First().Id);
			Assert.Equal(1, manifest.Metadata.DependencyGroups.First().Packages.First().Exclude.Count);
			Assert.Equal("all", manifest.Metadata.DependencyGroups.First().Packages.First().Exclude[0]);
		}

		[Fact]
		public void when_creating_package_with_dependency_and_without_include_assets_then_not_contains_dependency_include_attribute()
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
			Assert.Single(manifest.Metadata.DependencyGroups);
			Assert.Single(manifest.Metadata.DependencyGroups.First().Packages);
			Assert.Equal("Newtonsoft.Json", manifest.Metadata.DependencyGroups.First().Packages.First().Id);
			Assert.Equal(0, manifest.Metadata.DependencyGroups.First().Packages.First().Include.Count);
		}

		[Fact]
		public void when_creating_package_with_dependency_and_without_exclude_assets_then_not_contains_dependency_exclude_attribute()
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
			Assert.Single(manifest.Metadata.DependencyGroups);
			Assert.Single(manifest.Metadata.DependencyGroups.First().Packages);
			Assert.Equal("Newtonsoft.Json", manifest.Metadata.DependencyGroups.First().Packages.First().Id);
			Assert.Equal(0, manifest.Metadata.DependencyGroups.First().Packages.First().Exclude.Count);
		}

		[Fact]
		public void when_creating_package_with_non_framework_secific_dependency_then_contains_generic_dependency_group()
		{
			task.Contents = new[]
			{
				new TaskItem("Newtonsoft.Json", new Metadata
				{
					{ MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
					{ MetadataName.Kind, PackageItemKind.Dependency },
					{ MetadataName.Version, "8.0.0" },
					{ MetadataName.TargetFramework, "net45" },
					{ MetadataName.FrameworkSpecific, "false" },
				}),
				new TaskItem("Microsoft.Build", new Metadata
				{
					{ MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
					{ MetadataName.Kind, PackageItemKind.Dependency },
					{ MetadataName.Version, "15.0.0" },
					{ MetadataName.TargetFramework, "net45" }
				}),
			};

			var manifest = ExecuteTask();

			//Process.Start("notepad.exe", task.NuspecFile);

			Assert.NotNull(manifest);
			Assert.Equal(2, manifest.Metadata.DependencyGroups.Count());
			Assert.True(
				manifest.Metadata.DependencyGroups.First().TargetFramework == NuGetFramework.AnyFramework ||
				manifest.Metadata.DependencyGroups.First().TargetFramework == NuGetFramework.UnsupportedFramework);

			Assert.Equal(NuGetFramework.Parse(".NETFramework,Version=v4.5"), manifest.Metadata.DependencyGroups.Last().TargetFramework);

			Assert.Single(manifest.Metadata.DependencyGroups.First().Packages);
			Assert.Equal("Newtonsoft.Json", manifest.Metadata.DependencyGroups.First().Packages.First().Id);
			Assert.Equal("8.0.0", manifest.Metadata.DependencyGroups.First().Packages.First().VersionRange.MinVersion.ToString());

			Assert.Single(manifest.Metadata.DependencyGroups.Last().Packages);
			Assert.Equal("Microsoft.Build", manifest.Metadata.DependencyGroups.Last().Packages.First().Id);
			Assert.Equal("15.0.0", manifest.Metadata.DependencyGroups.Last().Packages.First().VersionRange.MinVersion.ToString());
		}

		[Fact]
		public void when_creating_package_with_any_framework_secific_dependency_then_contains_generic_dependency_group()
		{
			task.Contents = new[]
			{
				new TaskItem("Newtonsoft.Json", new Metadata
				{
					{ MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
					{ MetadataName.Kind, PackageItemKind.Dependency },
					{ MetadataName.Version, "8.0.0" },
					{ MetadataName.TargetFramework, "any" },
				}),
				new TaskItem("Microsoft.Build", new Metadata
				{
					{ MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
					{ MetadataName.Kind, PackageItemKind.Dependency },
					{ MetadataName.Version, "15.0.0" },
					{ MetadataName.TargetFramework, "net45" }
				}),
			};

			var manifest = ExecuteTask();

			Assert.NotNull(manifest);
			Assert.Equal(2, manifest.Metadata.DependencyGroups.Count());
			Assert.True(
				manifest.Metadata.DependencyGroups.First().TargetFramework == NuGetFramework.AnyFramework ||
				manifest.Metadata.DependencyGroups.First().TargetFramework == NuGetFramework.UnsupportedFramework);

			Assert.Equal(NuGetFramework.Parse(".NETFramework,Version=v4.5"), manifest.Metadata.DependencyGroups.Last().TargetFramework);

			Assert.Single(manifest.Metadata.DependencyGroups.First().Packages);
			Assert.Equal("Newtonsoft.Json", manifest.Metadata.DependencyGroups.First().Packages.First().Id);
			Assert.Equal("8.0.0", manifest.Metadata.DependencyGroups.First().Packages.First().VersionRange.MinVersion.ToString());

			Assert.Single(manifest.Metadata.DependencyGroups.Last().Packages);
			Assert.Equal("Microsoft.Build", manifest.Metadata.DependencyGroups.Last().Packages.First().Id);
			Assert.Equal("15.0.0", manifest.Metadata.DependencyGroups.Last().Packages.First().VersionRange.MinVersion.ToString());
		}

		[Fact]
		public void when_creating_package_with_empty_dependency_groups_then_succeeds()
		{
			task.Contents = new[]
			{
				new TaskItem(Path.GetTempFileName(), new Metadata
				{
					{ MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
					{ MetadataName.Kind, PackageItemKind.None },
					{ MetadataName.PackagePath, "readme.txt" }
				}),
				new TaskItem("_._", new Metadata
				{
					{ MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
					{ MetadataName.Kind, PackageItemKind.Dependency },
					{ MetadataName.Version, "*" },
					{ MetadataName.TargetFramework, "net45" }
				}),
				new TaskItem("_._", new Metadata
				{
					{ MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
					{ MetadataName.Kind, PackageItemKind.Dependency },
					{ MetadataName.Version, "*" },
					{ MetadataName.TargetFramework, "win" }
				}),
				new TaskItem("_._", new Metadata
				{
					{ MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
					{ MetadataName.Kind, PackageItemKind.Dependency },
					{ MetadataName.Version, "*" },
					{ MetadataName.TargetFramework, "wpa" }
				}),
				new TaskItem("_._", new Metadata
				{
					{ MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
					{ MetadataName.Kind, PackageItemKind.Dependency },
					{ MetadataName.Version, "*" },
					{ MetadataName.TargetFramework, "MonoAndroid10" }
				}),
			};

			task.NuspecFile = Path.GetTempFileName();
			createPackage = Debugger.IsAttached;

			var manifest = ExecuteTask();

			Assert.NotNull(manifest);
			Assert.Equal(4, manifest.Metadata.DependencyGroups.Count());
			Assert.All(manifest.Metadata.DependencyGroups, d => Assert.Empty(d.Packages));
		}

		[Fact]
		public void when_creating_package_with_multiple_target_frameworks_generates_empty_dependency_groups()
		{
			task.Contents = new[]
			{
				new TaskItem(Path.GetTempFileName(), new Metadata
				{
					{ MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
					{ MetadataName.PackagePath, "lib\\net35\\library.dll" },
					{ MetadataName.Kind, PackageItemKind.Lib },
					{ MetadataName.TargetFramework, "net35" }
				}),
				new TaskItem(Path.GetTempFileName(), new Metadata
				{
					{ MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
					{ MetadataName.PackagePath, "lib\\net40\\library.dll" },
					{ MetadataName.Kind, PackageItemKind.Lib },
					{ MetadataName.TargetFramework, "net40" }
				}),
				new TaskItem(Path.GetTempFileName(), new Metadata
				{
					{ MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
					{ MetadataName.PackagePath, "lib\\net45\\library.dll" },
					{ MetadataName.Kind, PackageItemKind.Lib },
					{ MetadataName.TargetFramework, "net45" }
				})
			};

			var manifest = ExecuteTask();

			HashSet<string> requiredFrameworks = new HashSet<string>()
			{
				"net35",
				"net40",
				"net45"
			};

			Assert.NotNull(manifest);
			Assert.Equal(3, manifest.Metadata.DependencyGroups.Count());
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
					{ MetadataName.PrivateAssets, "all" },
					// NOTE: AssignPackagePath takes care of converting TFM > short name
					{ MetadataName.TargetFramework, "net45" }
				}),
			};

			var manifest = ExecuteTask();

			Assert.Empty(manifest.Metadata.DependencyGroups);
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
			Assert.Single(manifest.Metadata.DependencyGroups);
			Assert.Equal(NuGetFramework.Parse(".NETFramework,Version=v4.5"), manifest.Metadata.DependencyGroups.First().TargetFramework);
			Assert.Single(manifest.Metadata.DependencyGroups.First().Packages);
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
			Assert.Contains(manifest.Metadata.ContentFiles, file => file.Include == @"any\any\readme.txt");
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
			Assert.Contains(manifest.Metadata.ContentFiles, file => file.Include == @"any\any\readme.txt" && file.BuildAction == "EmbeddedResource");
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
					new ManifestContentFiles { Include = @"any\any\readme.txt", CopyToOutput = "true" }
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
						Include = @"any\any\readme.txt",
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

		[Fact]
		public void when_creating_package_with_duplicate_framework_references_then_contains_only_unique_references()
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
					{ MetadataName.TargetFrameworkMoniker, TargetFrameworks.NET45 }
				}),
				new TaskItem("System.Xml", new Metadata
				{
					{ MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
					{ MetadataName.Kind, PackageItemKind.FrameworkReference },
					{ MetadataName.TargetFrameworkMoniker, TargetFrameworks.PCL78 }
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

		[Fact]
		public void when_creating_package_without_package_type()
		{
			task.Manifest = new TaskItem("package",
				new Metadata
				{
					{ "Id", "package" },
					{ "Version", "1.0.0" },
					{ "Title", "title" },
					{ "Description", "description" },
					{ "Authors", "author1, author2" },
				});

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
			Assert.Empty(metadata.PackageTypes);
		}

		[Fact]
		public void when_creating_package_with_package_type_empty()
		{
			task.Manifest = new TaskItem("package",
				new Metadata
				{
					{ "Id", "package" },
					{ "Version", "1.0.0" },
					{ "Title", "title" },
					{ "Description", "description" },
					{ "Authors", "author1, author2" },
					{ "PackageTypes", string.Empty }
				});

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
			Assert.Empty(metadata.PackageTypes);
		}

		[Fact]
		public void when_creating_package_with_package_type_version()
		{
			task.Manifest = new TaskItem("package",
				new Metadata
				{
					{ "Id", "package" },
					{ "Version", "1.0.0" },
					{ "Title", "title" },
					{ "Description", "description" },
					{ "Authors", "author1, author2" },
					{ "PackageTypes", "SomeType, 2.0.0" }
				});

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
			Assert.Collection(metadata.PackageTypes,
				item =>
				{
					Assert.Equal("SomeType", item.Name);
					Assert.Equal(new Version(2, 0, 0), item.Version);
				});
		}

		[Fact]
		public void when_creating_package_with_multiple_package_types()
		{
			task.Manifest = new TaskItem("package",
				new Metadata
				{
					{ "Id", "package" },
					{ "Version", "1.0.0" },
					{ "Title", "title" },
					{ "Description", "description" },
					{ "Authors", "author1, author2" },
					{ "PackageTypes", "SomeType, 2.0.0; AnotherType; ThirdTypeWithVersion, 1.2.3.4" }
				});

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
			Assert.Collection(metadata.PackageTypes,
				item =>
				{
					Assert.Equal("SomeType", item.Name);
					Assert.Equal(new Version(2, 0, 0), item.Version);
				},
				item =>
				{
					Assert.Equal("AnotherType", item.Name);
					Assert.Equal(PackageType.EmptyVersion, item.Version);
				},
				item =>
				{
					Assert.Equal("ThirdTypeWithVersion", item.Name);
					Assert.Equal(new Version(1, 2, 3, 4), item.Version);
				});
		}
	}
}
