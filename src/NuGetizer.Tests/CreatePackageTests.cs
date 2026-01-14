using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Packaging.Licenses;
using NuGetizer.Tasks;
using Xunit;
using Xunit.Abstractions;
using Metadata = System.Collections.Generic.Dictionary<string, string>;

namespace NuGetizer
{
    public class CreatePackageTests
    {
        readonly MockBuildEngine engine;
        readonly CreatePackage task;
        bool createPackage = false;

        public CreatePackageTests(ITestOutputHelper output)
        {
            engine = new MockBuildEngine(output);
            task = new CreatePackage
            {
                BuildEngine = engine,
                NuspecFile = Path.GetTempFileName(),
                Manifest = new TaskItem("package", new Metadata
                {
                    { MetadataName.PackageId, "package" },
                    { MetadataName.Version, "1.0.0" },
                    { "Title", "title $id$" },
                    { "Description",
                        """

                            This is the description.
                            Indent will be trimmed.


                            New paragraph preserved.

                        """ },
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
                }),
                ReplacementTokens =
                [
                    new TaskItem("Id", new Dictionary<string, string> { ["Value"] = "package" }),
                    new TaskItem("Version", new Dictionary<string, string> { ["Value"] = "1.0.0" }),
                    new TaskItem("Product", new Dictionary<string, string> { ["Value"] = "NuGetizer" }),
                ]
            };

#if RELEASE
            // Create the actual .nupkg to ensure everything is working 
            // fine end to end.
            createPackage = true;
#endif
        }

        Manifest ExecuteTask() => ExecuteTask(out _);

        Manifest ExecuteTask(out Manifest sourceManifest)
        {
            if (createPackage)
                return task.Execute(new MemoryStream(), out sourceManifest);

            sourceManifest = null;
            return task.CreateManifest();
        }

        [Fact]
        public void when_output_path_not_exists_then_creates_it()
        {
            task.Contents = new[]
            {
				// Need at least one dependency or content file for the generation to succeed.
				new TaskItem("Newtonsoft.Json", new Metadata
                {
                    { MetadataName.PackageId, "package" },
                    { MetadataName.PackFolder, PackFolderKind.Dependency },
                    { MetadataName.Version, "8.0.0" },
                    { MetadataName.TargetFramework, "net472" }
                }),
            };

            task.EmitPackage = "true";
            task.TargetPath = Path.Combine(Base62.Encode(DateTime.Now.Ticks), "output.nupkg");

            Assert.True(task.Execute());
            Assert.True(File.Exists(task.TargetPath));
        }


        [Fact]
        public void when_creating_package_then_contains_all_metadata()
        {
            task.Contents = new[]
            {
				// Need at least one dependency or content file for the generation to succeed.
				new TaskItem("Newtonsoft.Json", new Metadata
                {
                    { MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
                    { MetadataName.PackFolder, PackFolderKind.Dependency },
                    { MetadataName.Version, "8.0.0" },
                    { MetadataName.TargetFramework, "net472" }
                }),
            };

            var metadata = ExecuteTask().Metadata;

            Assert.True(metadata.RequireLicenseAcceptance);

            Assert.Equal(task.Manifest.GetMetadata("PackageId"), metadata.Id);
            Assert.Equal(task.Manifest.GetMetadata("Version"), metadata.Version.ToString());
            Assert.Equal("title package", metadata.Title);
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

            // C#-style triming is applied.
            Assert.Equal(
                """
                This is the description. Indent will be trimmed.

                New paragraph preserved.
                """.ReplaceLineEndings(),
                metadata.Description.ReplaceLineEndings());
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
                    { MetadataName.PackFolder, PackFolderKind.Dependency },
                    { MetadataName.Version, "8.0.0" },
                    { MetadataName.TargetFramework, "net472" }
                }),
            };

            task.Manifest.SetMetadata("DevelopmentDependency", "true");

            var metadata = ExecuteTask().Metadata;

            Assert.True(metadata.DevelopmentDependency);
        }

        [Fact]
        public void when_creating_package_has_license_expression_then_manifest_has_license()
        {
            task.Contents = new[]
            {
				// Need at least one dependency or content file for the generation to succeed.
				new TaskItem("Newtonsoft.Json", new Metadata
                {
                    { MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
                    { MetadataName.PackFolder, PackFolderKind.Dependency },
                    { MetadataName.Version, "8.0.0" },
                    { MetadataName.TargetFramework, "net472" }
                }),
            };

            task.Manifest.SetMetadata("LicenseUrl", "");
            task.Manifest.SetMetadata("LicenseExpression", "MIT");

            var metadata = ExecuteTask().Metadata;

            Assert.Equal("MIT", metadata.LicenseMetadata.License);
            Assert.Equal(LicenseType.Expression, metadata.LicenseMetadata.Type);
            Assert.Equal(LicenseExpressionType.License, metadata.LicenseMetadata.LicenseExpression.Type);
            Assert.True(metadata.LicenseMetadata.LicenseExpression is NuGetLicense license && license.IsStandardLicense);
        }

        [Fact]
        public void when_creating_package_has_license_file_then_manifest_has_license()
        {
            var content = Path.GetTempFileName();
            task.Contents = new[]
            {
                new TaskItem(content, new Metadata
                {
                    { MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
                    { MetadataName.PackFolder, PackFolderKind.None },
                    { MetadataName.PackagePath, "license.txt" }
                }),
            };

            task.Manifest.SetMetadata("LicenseUrl", "");
            task.Manifest.SetMetadata("LicenseFile", "license.txt");

            var metadata = ExecuteTask().Metadata;

            Assert.Equal("license.txt", metadata.LicenseMetadata.License);
            Assert.Equal(LicenseType.File, metadata.LicenseMetadata.Type);
            Assert.Null(metadata.LicenseMetadata.LicenseExpression);
            Assert.Null(metadata.LicenseMetadata.WarningsAndErrors);
        }

        [Fact]
        public void when_license_file_has_tokens_then_replacements_applied()
        {
            var content = Path.GetTempFileName();
            File.WriteAllText(content, "EULA for $product$ ($id$).");
            task.Contents = new[]
            {
                new TaskItem(content, new Metadata
                {
                    { MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
                    { MetadataName.PackFolder, PackFolderKind.None },
                    { MetadataName.PackagePath, "license.txt" }
                }),
            };

            task.Manifest.SetMetadata("LicenseUrl", "");
            task.Manifest.SetMetadata("LicenseFile", "license.txt");

            createPackage = true;
            ExecuteTask(out var manifest);

            Assert.NotNull(manifest);

            Assert.Equal("license.txt", manifest.Metadata.LicenseMetadata.License);
            Assert.Equal(LicenseType.File, manifest.Metadata.LicenseMetadata.Type);

            var file = manifest.Files.FirstOrDefault(f => Path.GetFileName(f.Target) == manifest.Metadata.LicenseMetadata.License);
            Assert.NotNull(file);
            Assert.True(File.Exists(file.Source));

            var eula = File.ReadAllText(file.Source);

            Assert.Equal("EULA for NuGetizer (package).", eula);
        }

        [Fact]
        public void when_readme_has_include_and_tokens_then_replacements_applied()
        {
            var content = Path.GetTempFileName();
            File.WriteAllText(content, "<!-- include https://github.com/devlooped/.github/blob/807335297e28cfe5a6dd00ecd72b2ca32c0f1ed8/osmf.md -->");
            task.Contents = new[]
            {
                new TaskItem(content, new Metadata
                {
                    { MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
                    { MetadataName.PackFolder, PackFolderKind.None },
                    { MetadataName.PackagePath, "readme.md" }
                }),
            };

            task.Manifest.SetMetadata("Readme", "readme.md");

            createPackage = true;
            ExecuteTask(out var manifest);

            Assert.NotNull(manifest);

            Assert.Equal("readme.md", manifest.Metadata.Readme);

            var file = manifest.Files.FirstOrDefault(f => Path.GetFileName(f.Target) == manifest.Metadata.Readme);
            Assert.NotNull(file);
            Assert.True(File.Exists(file.Source));

            var readme = File.ReadAllText(file.Source);

            Assert.Contains("NuGetizer", readme);
        }

        [Fact]
        public void when_readme_has_relativeurl_then_expands_github_url()
        {
            var content = Path.GetTempFileName();
            File.WriteAllText(content, "See [license](license.txt).");
            task.Contents = new[]
            {
                new TaskItem(content, new Metadata
                {
                    { MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
                    { MetadataName.PackFolder, PackFolderKind.None },
                    { MetadataName.PackagePath, "readme.md" }
                }),
            };

            task.Manifest.SetMetadata("Readme", "readme.md");
            task.Manifest.SetMetadata("RepositoryType", "git");
            task.Manifest.SetMetadata("RepositoryUrl", "https://github.com/devlooped/nugetizer");
            task.Manifest.SetMetadata("RepositorySha", "9dc2cb5de");

            createPackage = true;
            ExecuteTask(out var manifest);

            Assert.NotNull(manifest);

            Assert.Equal("readme.md", manifest.Metadata.Readme);

            var file = manifest.Files.FirstOrDefault(f => Path.GetFileName(f.Target) == manifest.Metadata.Readme);
            Assert.NotNull(file);
            Assert.True(File.Exists(file.Source));

            var readme = File.ReadAllText(file.Source);

            Assert.Contains("[license](https://raw.githubusercontent.com/devlooped/nugetizer/9dc2cb5de/license.txt)", readme);
        }

        [Fact]
        public void when_readme_has_link_with_tooltip_then_preserves_tooltip()
        {
            var content = Path.GetTempFileName();
            File.WriteAllText(content, "See ![avatar](avatars/user.png \"User Avatar\").");
            task.Contents = new[]
            {
                new TaskItem(content, new Metadata
                {
                    { MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
                    { MetadataName.PackFolder, PackFolderKind.None },
                    { MetadataName.PackagePath, "readme.md" }
                }),
            };

            task.Manifest.SetMetadata("Readme", "readme.md");
            task.Manifest.SetMetadata("RepositoryType", "git");
            task.Manifest.SetMetadata("RepositoryUrl", "https://github.com/devlooped/nugetizer");
            task.Manifest.SetMetadata("RepositorySha", "abc123def");

            createPackage = true;
            ExecuteTask(out var manifest);

            Assert.NotNull(manifest);

            var file = manifest.Files.FirstOrDefault(f => Path.GetFileName(f.Target) == manifest.Metadata.Readme);
            Assert.NotNull(file);
            Assert.True(File.Exists(file.Source));

            var readme = File.ReadAllText(file.Source);

            Assert.Contains("[avatar](https://raw.githubusercontent.com/devlooped/nugetizer/abc123def/avatars/user.png \"User Avatar\")", readme);
        }

        [Fact]
        public void when_readme_has_absolute_url_then_does_not_replace()
        {
            var content = Path.GetTempFileName();
            File.WriteAllText(content, "[![badge](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/user.png \"User\")](https://github.com/user)");
            task.Contents = new[]
            {
                new TaskItem(content, new Metadata
                {
                    { MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
                    { MetadataName.PackFolder, PackFolderKind.None },
                    { MetadataName.PackagePath, "readme.md" }
                }),
            };

            task.Manifest.SetMetadata("Readme", "readme.md");
            task.Manifest.SetMetadata("RepositoryType", "git");
            task.Manifest.SetMetadata("RepositoryUrl", "https://github.com/devlooped/nugetizer");
            task.Manifest.SetMetadata("RepositorySha", "abc123def");

            createPackage = true;
            ExecuteTask(out var manifest);

            Assert.NotNull(manifest);

            var file = manifest.Files.FirstOrDefault(f => Path.GetFileName(f.Target) == manifest.Metadata.Readme);
            Assert.NotNull(file);
            Assert.True(File.Exists(file.Source));

            var readme = File.ReadAllText(file.Source);

            // Should NOT prepend repository URL to absolute URLs
            Assert.DoesNotContain("https://raw.githubusercontent.com/devlooped/nugetizer/abc123def/https://raw.githubusercontent.com", readme);
            // Should preserve the original absolute URL
            Assert.Contains("https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/user.png", readme);
        }

        [Fact]
        public void when_readme_has_image_link_then_uses_raw_url()
        {
            var content = Path.GetTempFileName();
            File.WriteAllText(content, "![Image](img/logo.png)");
            task.Contents = new[]
            {
                new TaskItem(content, new Metadata
                {
                    { MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
                    { MetadataName.PackFolder, PackFolderKind.None },
                    { MetadataName.PackagePath, "readme.md" }
                }),
            };

            task.Manifest.SetMetadata("Readme", "readme.md");
            task.Manifest.SetMetadata("RepositoryType", "git");
            task.Manifest.SetMetadata("RepositoryUrl", "https://github.com/devlooped/nugetizer");
            task.Manifest.SetMetadata("RepositorySha", "abc123def");

            createPackage = true;
            ExecuteTask(out var manifest);

            Assert.NotNull(manifest);

            var file = manifest.Files.FirstOrDefault(f => Path.GetFileName(f.Target) == manifest.Metadata.Readme);
            Assert.NotNull(file);
            Assert.True(File.Exists(file.Source));

            var readme = File.ReadAllText(file.Source);

            // Should use raw.githubusercontent.com format for proper image display
            Assert.Contains("https://raw.githubusercontent.com/devlooped/nugetizer/abc123def/img/logo.png", readme);
            Assert.DoesNotContain("/blob/", readme);
        }

        [Fact]
        public void when_readme_has_relative_image_url_and_link_then_expands_both()
        {
            var content = Path.GetTempFileName();
            File.WriteAllText(content, "[![Image](img/logo.png)](osmf.txt)");
            task.Contents = new[]
            {
                new TaskItem(content, new Metadata
                {
                    { MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
                    { MetadataName.PackFolder, PackFolderKind.None },
                    { MetadataName.PackagePath, "readme.md" }
                }),
            };

            task.Manifest.SetMetadata("Readme", "readme.md");
            task.Manifest.SetMetadata("RepositoryType", "git");
            task.Manifest.SetMetadata("RepositoryUrl", "https://github.com/devlooped/nugetizer");
            task.Manifest.SetMetadata("RepositorySha", "abc123def");

            createPackage = true;
            ExecuteTask(out var manifest);

            Assert.NotNull(manifest);

            var file = manifest.Files.FirstOrDefault(f => Path.GetFileName(f.Target) == manifest.Metadata.Readme);
            Assert.NotNull(file);
            Assert.True(File.Exists(file.Source));

            var readme = File.ReadAllText(file.Source);

            // Should use raw.githubusercontent.com format for proper image display
            Assert.Contains("https://raw.githubusercontent.com/devlooped/nugetizer/abc123def/img/logo.png", readme);
            Assert.Contains("https://raw.githubusercontent.com/devlooped/nugetizer/abc123def/osmf.txt", readme);
            Assert.DoesNotContain("/blob/", readme);
        }


        [Fact]
        public void when_readme_has_clickable_image_badge_with_relative_url_then_replaces_url()
        {
            var content = Path.GetTempFileName();
            File.WriteAllText(content, "[![EULA](https://img.shields.io/badge/EULA-OSMF-blue)](osmfeula.txt)");
            task.Contents = new[]
            {
                new TaskItem(content, new Metadata
                {
                    { MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
                    { MetadataName.PackFolder, PackFolderKind.None },
                    { MetadataName.PackagePath, "readme.md" }
                }),
            };

            task.Manifest.SetMetadata("Readme", "readme.md");
            task.Manifest.SetMetadata("RepositoryType", "git");
            task.Manifest.SetMetadata("RepositoryUrl", "https://github.com/devlooped/nugetizer");
            task.Manifest.SetMetadata("RepositorySha", "abc123def");

            createPackage = true;
            ExecuteTask(out var manifest);

            Assert.NotNull(manifest);

            var file = manifest.Files.FirstOrDefault(f => Path.GetFileName(f.Target) == manifest.Metadata.Readme);
            Assert.NotNull(file);
            Assert.True(File.Exists(file.Source));

            var readme = File.ReadAllText(file.Source);

            // Should replace the relative URL in the clickable image badge
            Assert.Contains("https://raw.githubusercontent.com/devlooped/nugetizer/abc123def/osmfeula.txt", readme);
            // Should preserve the absolute image URL
            Assert.Contains("https://img.shields.io/badge/EULA-OSMF-blue", readme);
        }

        [Fact]
        public void when_creating_package_with_simple_dependency_then_contains_dependency_group()
        {
            task.Contents = new[]
            {
                new TaskItem("Newtonsoft.Json", new Metadata
                {
                    { MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
                    { MetadataName.PackFolder, PackFolderKind.Dependency },
                    { MetadataName.Version, "8.0.0" },
					// NOTE: AssignPackagePath takes care of converting TFM > short name
					{ MetadataName.TargetFramework, "net472" }
                }),
            };

            var manifest = ExecuteTask();

            Assert.NotNull(manifest);
            Assert.Single(manifest.Metadata.DependencyGroups);
            Assert.Equal(NuGetFramework.Parse(".NETFramework,Version=v4.7.2"), manifest.Metadata.DependencyGroups.First().TargetFramework);
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
                    { MetadataName.PackFolder, PackFolderKind.Dependency },
                    { MetadataName.Version, "8.0.0" },
					// NOTE: AssignPackagePath takes care of converting TFM > short name
					{ MetadataName.TargetFramework, "net472" },
                    { MetadataName.IncludeAssets, "all" }
                }),
            };

            var manifest = ExecuteTask();

            Assert.NotNull(manifest);
            Assert.Single(manifest.Metadata.DependencyGroups);
            Assert.Single(manifest.Metadata.DependencyGroups.First().Packages);
            Assert.Equal("Newtonsoft.Json", manifest.Metadata.DependencyGroups.First().Packages.First().Id);
            Assert.Single(manifest.Metadata.DependencyGroups.First().Packages.First().Include);
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
                    { MetadataName.PackFolder, PackFolderKind.Dependency },
                    { MetadataName.Version, "8.0.0" },
					// NOTE: AssignPackagePath takes care of converting TFM > short name
					{ MetadataName.TargetFramework, "net472" },
                    { MetadataName.ExcludeAssets, "all" }
                }),
            };

            var manifest = ExecuteTask();

            Assert.NotNull(manifest);
            Assert.Single(manifest.Metadata.DependencyGroups);
            Assert.Single(manifest.Metadata.DependencyGroups.First().Packages);
            Assert.Equal("Newtonsoft.Json", manifest.Metadata.DependencyGroups.First().Packages.First().Id);
            Assert.Single(manifest.Metadata.DependencyGroups.First().Packages.First().Exclude);
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
                    { MetadataName.PackFolder, PackFolderKind.Dependency },
                    { MetadataName.Version, "8.0.0" },
					// NOTE: AssignPackagePath takes care of converting TFM > short name
					{ MetadataName.TargetFramework, "net472" }
                }),
            };

            var manifest = ExecuteTask();

            Assert.NotNull(manifest);
            Assert.Single(manifest.Metadata.DependencyGroups);
            Assert.Single(manifest.Metadata.DependencyGroups.First().Packages);
            Assert.Equal("Newtonsoft.Json", manifest.Metadata.DependencyGroups.First().Packages.First().Id);
            Assert.Empty(manifest.Metadata.DependencyGroups.First().Packages.First().Include);
        }

        [Fact]
        public void when_creating_package_with_dependency_and_without_exclude_assets_then_not_contains_dependency_exclude_attribute()
        {
            task.Contents = new[]
            {
                new TaskItem("Newtonsoft.Json", new Metadata
                {
                    { MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
                    { MetadataName.PackFolder, PackFolderKind.Dependency },
                    { MetadataName.Version, "8.0.0" },
					// NOTE: AssignPackagePath takes care of converting TFM > short name
					{ MetadataName.TargetFramework, "net472" }
                }),
            };

            var manifest = ExecuteTask();

            Assert.NotNull(manifest);
            Assert.Single(manifest.Metadata.DependencyGroups);
            Assert.Single(manifest.Metadata.DependencyGroups.First().Packages);
            Assert.Equal("Newtonsoft.Json", manifest.Metadata.DependencyGroups.First().Packages.First().Id);
            Assert.Empty(manifest.Metadata.DependencyGroups.First().Packages.First().Exclude);
        }

        [Fact]
        public void when_creating_package_with_dependency_packinclude_then_contains_dependency_include_attribute()
        {
            task.Contents = new[]
            {
                new TaskItem("Newtonsoft.Json", new Metadata
                {
                    { MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
                    { MetadataName.PackFolder, PackFolderKind.Dependency },
                    { MetadataName.Version, "8.0.0" },
					// NOTE: AssignPackagePath takes care of converting TFM > short name
					{ MetadataName.TargetFramework, "net472" },
                    { MetadataName.PackInclude, "build" }
                }),
            };

            var manifest = ExecuteTask();

            Assert.NotNull(manifest);
            Assert.Single(manifest.Metadata.DependencyGroups);
            Assert.Single(manifest.Metadata.DependencyGroups.First().Packages);
            Assert.Equal("Newtonsoft.Json", manifest.Metadata.DependencyGroups.First().Packages.First().Id);
            Assert.Single(manifest.Metadata.DependencyGroups.First().Packages.First().Include);
            Assert.Equal("build", manifest.Metadata.DependencyGroups.First().Packages.First().Include[0]);
        }

        [Fact]
        public void when_creating_package_with_dependency_packexclude_assets_then_contains_dependency_exclude_attribute()
        {
            task.Contents = new[]
            {
                new TaskItem("Newtonsoft.Json", new Metadata
                {
                    { MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
                    { MetadataName.PackFolder, PackFolderKind.Dependency },
                    { MetadataName.Version, "8.0.0" },
					// NOTE: AssignPackagePath takes care of converting TFM > short name
					{ MetadataName.TargetFramework, "net472" },
                    { MetadataName.PackExclude, "build" }
                }),
            };

            var manifest = ExecuteTask();

            Assert.NotNull(manifest);
            Assert.Single(manifest.Metadata.DependencyGroups);
            Assert.Single(manifest.Metadata.DependencyGroups.First().Packages);
            Assert.Equal("Newtonsoft.Json", manifest.Metadata.DependencyGroups.First().Packages.First().Id);
            Assert.Single(manifest.Metadata.DependencyGroups.First().Packages.First().Exclude);
            Assert.Equal("build", manifest.Metadata.DependencyGroups.First().Packages.First().Exclude[0]);
        }

        [Fact]
        public void when_creating_package_with_non_framework_secific_dependency_then_contains_generic_dependency_group()
        {
            task.Contents = new[]
            {
                new TaskItem("Newtonsoft.Json", new Metadata
                {
                    { MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
                    { MetadataName.PackFolder, PackFolderKind.Dependency },
                    { MetadataName.Version, "8.0.0" },
                    { MetadataName.TargetFramework, "net472" },
                    { MetadataName.FrameworkSpecific, "false" },
                }),
                new TaskItem("Microsoft.Build", new Metadata
                {
                    { MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
                    { MetadataName.PackFolder, PackFolderKind.Dependency },
                    { MetadataName.Version, "15.0.0" },
                    { MetadataName.TargetFramework, "net472" }
                }),
            };

            var manifest = ExecuteTask();

            //Process.Start("notepad.exe", task.NuspecFile);

            Assert.NotNull(manifest);
            Assert.Equal(2, manifest.Metadata.DependencyGroups.Count());
            Assert.True(
                manifest.Metadata.DependencyGroups.First().TargetFramework == NuGetFramework.AnyFramework ||
                manifest.Metadata.DependencyGroups.First().TargetFramework == NuGetFramework.UnsupportedFramework);

            Assert.Equal(NuGetFramework.Parse(".NETFramework,Version=v4.7.2"), manifest.Metadata.DependencyGroups.Last().TargetFramework);

            Assert.Single(manifest.Metadata.DependencyGroups.First().Packages);
            Assert.Equal("Newtonsoft.Json", manifest.Metadata.DependencyGroups.First().Packages.First().Id);
            Assert.Equal("8.0.0", manifest.Metadata.DependencyGroups.First().Packages.First().VersionRange.MinVersion.ToString());

            Assert.Single(manifest.Metadata.DependencyGroups.Last().Packages);
            Assert.Equal("Microsoft.Build", manifest.Metadata.DependencyGroups.Last().Packages.First().Id);
            Assert.Equal("15.0.0", manifest.Metadata.DependencyGroups.Last().Packages.First().VersionRange.MinVersion.ToString());
        }

        [Fact]
        public void when_creating_package_with_any_framework_specific_dependency_then_contains_generic_dependency_group()
        {
            task.Contents = new[]
            {
                new TaskItem("Newtonsoft.Json", new Metadata
                {
                    { MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
                    { MetadataName.PackFolder, PackFolderKind.Dependency },
                    { MetadataName.Version, "8.0.0" },
                    { MetadataName.TargetFramework, "any" },
                }),
                new TaskItem("Microsoft.Build", new Metadata
                {
                    { MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
                    { MetadataName.PackFolder, PackFolderKind.Dependency },
                    { MetadataName.Version, "15.0.0" },
                    { MetadataName.TargetFramework, "net472" }
                }),
            };

            var manifest = ExecuteTask();

            Assert.NotNull(manifest);
            Assert.Equal(2, manifest.Metadata.DependencyGroups.Count());
            Assert.True(
                manifest.Metadata.DependencyGroups.First().TargetFramework == NuGetFramework.AnyFramework ||
                manifest.Metadata.DependencyGroups.First().TargetFramework == NuGetFramework.UnsupportedFramework);

            Assert.Equal(NuGetFramework.Parse(".NETFramework,Version=v4.7.2"), manifest.Metadata.DependencyGroups.Last().TargetFramework);

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
                    { MetadataName.PackFolder, PackFolderKind.None },
                    { MetadataName.PackagePath, "readme.txt" }
                }),
                new TaskItem("_._", new Metadata
                {
                    { MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
                    { MetadataName.PackFolder, PackFolderKind.Dependency },
                    { MetadataName.Version, "*" },
                    { MetadataName.TargetFramework, "net472" }
                }),
                new TaskItem("_._", new Metadata
                {
                    { MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
                    { MetadataName.PackFolder, PackFolderKind.Dependency },
                    { MetadataName.Version, "*" },
                    { MetadataName.TargetFramework, "win" }
                }),
                new TaskItem("_._", new Metadata
                {
                    { MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
                    { MetadataName.PackFolder, PackFolderKind.Dependency },
                    { MetadataName.Version, "*" },
                    { MetadataName.TargetFramework, "wpa" }
                }),
                new TaskItem("_._", new Metadata
                {
                    { MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
                    { MetadataName.PackFolder, PackFolderKind.Dependency },
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
                    { MetadataName.PackFolder, PackFolderKind.Lib },
                    { MetadataName.TargetFramework, "net35" }
                }),
                new TaskItem(Path.GetTempFileName(), new Metadata
                {
                    { MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
                    { MetadataName.PackagePath, "lib\\net40\\library.dll" },
                    { MetadataName.PackFolder, PackFolderKind.Lib },
                    { MetadataName.TargetFramework, "net40" }
                }),
                new TaskItem(Path.GetTempFileName(), new Metadata
                {
                    { MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
                    { MetadataName.PackagePath, "lib\\net472\\library.dll" },
                    { MetadataName.PackFolder, PackFolderKind.Lib },
                    { MetadataName.TargetFramework, "net472" }
                })
            };

            var manifest = ExecuteTask();

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
                    { MetadataName.PackFolder, PackFolderKind.None },
                    { MetadataName.PackagePath, "readme.txt" }
                }),
                new TaskItem("Helpers", new Metadata
                {
                    { MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
                    { MetadataName.Version, "1.0.0" },
                    { MetadataName.PackFolder, PackFolderKind.Dependency },
                    { MetadataName.PrivateAssets, "all" },
					// NOTE: AssignPackagePath takes care of converting TFM > short name
					{ MetadataName.TargetFramework, "net472" }
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
                    { MetadataName.PackFolder, PackFolderKind.Dependency },
                    { MetadataName.Version, "8.0.0" },
					// NOTE: AssignPackagePath takes care of converting TFM > short name
					{ MetadataName.TargetFramework, "net472" }
                }),
            };

            var manifest = ExecuteTask();

            Assert.NotNull(manifest);
            Assert.Single(manifest.Metadata.DependencyGroups);
            Assert.Equal(NuGetFramework.Parse(".NETFramework,Version=v4.7.2"), manifest.Metadata.DependencyGroups.First().TargetFramework);
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
                    { MetadataName.PackFolder, PackFolderKind.None },
                    { MetadataName.PackagePath, "readme.txt" }
                }),
            };

            var manifest = ExecuteTask();

            Assert.NotNull(manifest);
            Assert.Contains(manifest.Files, file => file.Target == "readme.txt");
        }

        [Fact]
        public void when_creating_package_with_readme_then_resolves_includes()
        {
            var readme = Path.GetTempFileName();
            var footer = Path.GetTempFileName();
            File.WriteAllText(footer, "footer");
            File.WriteAllText(readme, $"<!-- include {Path.GetFileName(footer)} -->");

            task.Manifest.SetMetadata("Readme", "readme.md");
            task.Contents = new[]
            {
                new TaskItem(readme, new Metadata
                {
                    { MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
                    { MetadataName.PackFolder, PackFolderKind.None },
                    { MetadataName.PackagePath, "readme.md" }
                }),
            };

            var tmp = Path.GetTempFileName();
            using (var file = File.Open(tmp, FileMode.OpenOrCreate))
                task.Execute(file);

            var zip = ZipFile.Open(tmp, ZipArchiveMode.Update);

            Assert.Contains(zip.Entries, entry => entry.Name == "readme.md");

            var entry = zip.Entries.Single(e => e.Name == "readme.md");
            var updated = Path.GetTempFileName();
            entry.ExtractToFile(updated, true);
            var content = File.ReadAllText(updated);

            Assert.Contains("footer", content);
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
                    { MetadataName.PackFolder, PackFolderKind.ContentFiles },
                    { MetadataName.PackageFolder, PackagingConstants.Folders.ContentFiles },
                    { MetadataName.PackagePath, "contentFiles/any/any/readme.txt" }
                }),
            };

            var manifest = ExecuteTask();

            Assert.Contains(manifest.Files, file => file.Target == "contentFiles/any/any/readme.txt");
            Assert.Contains(manifest.Metadata.ContentFiles, file => file.Include == "any/any/readme.txt");
        }

        [Fact]
        public void when_creating_package_with_readme_then_has_readme_metadata()
        {
            var content = Path.GetTempFileName();
            File.WriteAllText(content, "# Readme Sample");
            task.Contents = new[]
            {
                new TaskItem(content, new Metadata
                {
                    { MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
                    { MetadataName.PackagePath, @"readme.md" }
                }),
            };

            task.Manifest.SetMetadata("Readme", "readme.md");

            var manifest = ExecuteTask();

            Assert.Contains(manifest.Files, file => file.Target == @"readme.md");
            Assert.Equal("readme.md", manifest.Metadata.Readme);
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
                    { MetadataName.PackFolder, PackFolderKind.ContentFiles },
                    { MetadataName.PackageFolder, PackagingConstants.Folders.ContentFiles },
                    { MetadataName.ContentFile.BuildAction, "EmbeddedResource" },
                    { MetadataName.PackagePath, "contentFiles/any/any/readme.txt" }
                }),
            };

            var manifest = ExecuteTask();

            Assert.Contains(manifest.Files, file => file.Target == "contentFiles/any/any/readme.txt");
            Assert.Contains(manifest.Metadata.ContentFiles, file => file.Include == "any/any/readme.txt" && file.BuildAction == "EmbeddedResource");
        }

        [Fact]
        public void when_creating_package_with_content_file_copy_to_output_then_adds_as_content_file()
        {
            var content = Path.GetTempFileName();
            task.Contents = new[]
            {
                new TaskItem(content, new Metadata
                {
                    { MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
                    { MetadataName.PackFolder, PackFolderKind.ContentFiles },
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
            var content = Path.GetTempFileName();
            task.Contents = new[]
            {
                new TaskItem(content, new Metadata
                {
                    { MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
                    { MetadataName.PackFolder, PackFolderKind.ContentFiles },
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
                manifest.Files, ManifestFileComparer.Default);

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
                    { MetadataName.PackFolder, PackFolderKind.FrameworkReference },
                    { MetadataName.DefaultTargetFramework, TargetFrameworks.NET472 }
                }),
                new TaskItem("System.Xml", new Metadata
                {
                    { MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
                    { MetadataName.PackFolder, PackFolderKind.FrameworkReference },
                    { MetadataName.DefaultTargetFramework, TargetFrameworks.PCL78 }
                }),
            };

            var manifest = ExecuteTask();

            Assert.Equal(manifest.Metadata.FrameworkReferences,
                new[]
                {
                    new FrameworkAssemblyReference("System.Xml", new [] { NuGetFramework.Parse(TargetFrameworks.NET472) }),
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
                    { MetadataName.PackFolder, PackFolderKind.FrameworkReference },
                    { MetadataName.DefaultTargetFramework, TargetFrameworks.NET472 }
                }),
                new TaskItem("System.Xml", new Metadata
                {
                    { MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
                    { MetadataName.PackFolder, PackFolderKind.FrameworkReference },
                    { MetadataName.DefaultTargetFramework, TargetFrameworks.NET472 }
                }),
                new TaskItem("System.Xml", new Metadata
                {
                    { MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
                    { MetadataName.PackFolder, PackFolderKind.FrameworkReference },
                    { MetadataName.DefaultTargetFramework, TargetFrameworks.PCL78 }
                }),
                new TaskItem("System.Xml", new Metadata
                {
                    { MetadataName.PackageId, task.Manifest.GetMetadata("Id") },
                    { MetadataName.PackFolder, PackFolderKind.FrameworkReference },
                    { MetadataName.DefaultTargetFramework, TargetFrameworks.PCL78 }
                }),
            };

            var manifest = ExecuteTask();

            Assert.Equal(manifest.Metadata.FrameworkReferences,
                new[]
                {
                    new FrameworkAssemblyReference("System.Xml", new [] { NuGetFramework.Parse(TargetFrameworks.NET472) }),
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
                    { MetadataName.PackFolder, PackFolderKind.Dependency },
                    { MetadataName.Version, "8.0.0" },
                    { MetadataName.TargetFramework, "net472" }
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
                    { MetadataName.PackFolder, PackFolderKind.Dependency },
                    { MetadataName.Version, "8.0.0" },
                    { MetadataName.TargetFramework, "net472" }
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
                    { MetadataName.PackFolder, PackFolderKind.Dependency },
                    { MetadataName.Version, "8.0.0" },
                    { MetadataName.TargetFramework, "net472" }
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
                    { MetadataName.PackFolder, PackFolderKind.Dependency },
                    { MetadataName.Version, "8.0.0" },
                    { MetadataName.TargetFramework, "net472" }
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
