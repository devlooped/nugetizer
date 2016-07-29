using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Utilities;
using NuGet.Frameworks;
using NuGet.Packaging.Tasks.Tests.Infrastructure;
using NuGet.Versioning;
using Xunit;

namespace NuGet.Packaging.Tasks.Tests
{
    public class GenerateNuSpecTests : IDisposable
    {
        private string projectDirectory;

        public GenerateNuSpecTests()
        {
            var tempPath = Path.GetTempPath();
            var randomFileName = Path.GetRandomFileName();
            projectDirectory = Path.Combine(tempPath, randomFileName);
        }

        public void Dispose()
        {
            Directory.Delete(projectDirectory, true);
        }

        [Fact]
        public void GenerateNuSpec_OverrideInputFileName()
        {
            var input = Assets.GetScenarioFilePath("GenerateNuSpec", "NuGetPackage.nuspec");
            var output = Path.Combine(projectDirectory, "OverrideInputFileName.nuspec");

            var target = new GenerateNuSpec();
            target.InputFileName = input;
            target.OutputFileName = output;
            target.Id = "NuGetPackage2";
            target.Version = "2.0.0";
            target.Title = "NuGetPackage2";
            target.Authors = "Pavol";
            target.RequireLicenseAcceptance = true;
            target.Description = "NuGetPackage2";
            target.ReleaseNotes = "Released! (Again)";
            target.Summary = "NuGetPackage2";
            target.Language = "sk-sk";
            target.ProjectUrl = "http://nuproj.net/changes";
            target.IconUrl = "http://placekitten.com/g/128/128";
            target.LicenseUrl = "http://nuproj.net/LICENSE/changes";
            target.Copyright = "Copyright © Pavol";
            target.Tags = "NuGetPackage2";
            target.DevelopmentDependency = false;

            var fileToAdd = Path.Combine(projectDirectory, "SomeProject.dll");
            target.Files = new[] {
                new TaskItem(fileToAdd, new Dictionary<string, string>
                    {
                        {Metadata.FileSource, fileToAdd},
                        {Metadata.FileTarget, @"lib\net45"},
                    })
            };
            var result = target.Execute();

            target.OutputFileName = output;
            Assert.True(result);

            using (var stream = File.OpenRead(output))
            {
                var manifest = Manifest.ReadFrom(stream, false);
                Assert.Equal(target.Id, manifest.Metadata.Id);
                Assert.Equal(target.Version, manifest.Metadata.Version.ToString());
                Assert.Equal(target.Title, manifest.Metadata.Title);
                Assert.Equal(target.Authors, String.Join(",", manifest.Metadata.Authors));
                Assert.Equal(target.RequireLicenseAcceptance, manifest.Metadata.RequireLicenseAcceptance);
                Assert.Equal(target.Description, manifest.Metadata.Description);
                Assert.Equal(target.ReleaseNotes, manifest.Metadata.ReleaseNotes);
                Assert.Equal(target.Summary, manifest.Metadata.Summary);
                Assert.Equal(target.Language, manifest.Metadata.Language);
                Assert.Equal(target.ProjectUrl, manifest.Metadata.ProjectUrl.ToString());
                Assert.Equal(target.IconUrl, manifest.Metadata.IconUrl.ToString());
                Assert.Equal(target.LicenseUrl, manifest.Metadata.LicenseUrl.ToString());
                Assert.Equal(target.Copyright, manifest.Metadata.Copyright);
                Assert.Equal(target.Tags, manifest.Metadata.Tags);
                Assert.Equal(true, manifest.Metadata.DevelopmentDependency);

                var expectedFiles = new[] {
                    new ManifestFile
                    {
                        Source = "Readme.txt",
                        Target = "",
                        Exclude = "",
                    },
                    new ManifestFile
                    {
                        Source = fileToAdd,
                        Target = @"lib\net45",
                        Exclude = "",
                    },
                };

                Assert.Equal(expectedFiles,
                    manifest.Files,
                    ManifestFileComparer.Instance);
            }
        }

        [Fact]
        public void GenerateNuSpec_UseInputFileName()
        {
            var input = Assets.GetScenarioFilePath("GenerateNuSpec", "NuGetPackage.nuspec");
            var output = Path.Combine(projectDirectory, "UseInputFileName.nuspec");

            var target = new GenerateNuSpec();
            target.InputFileName = input;
            target.OutputFileName = output;
            var result = target.Execute();

            target.OutputFileName = output;
            Assert.True(result);

            using (var stream = File.OpenRead(output))
            {
                var manifest = Manifest.ReadFrom(stream, false);
                Assert.Equal("NuGetPackage", manifest.Metadata.Id);
                Assert.Equal("1.0.0", manifest.Metadata.Version.ToString());
                Assert.Equal("NuGetPackage", manifest.Metadata.Title);
                Assert.Equal("Immo", String.Join(",", manifest.Metadata.Authors));
                Assert.Equal(false, manifest.Metadata.RequireLicenseAcceptance);
                Assert.Equal("NuGetPackage", manifest.Metadata.Description);
                Assert.Equal("Released!", manifest.Metadata.ReleaseNotes);
                Assert.Equal("NuGetPackage", manifest.Metadata.Summary);
                Assert.Equal("en-us", manifest.Metadata.Language);
                Assert.Equal("http://nuproj.net/", manifest.Metadata.ProjectUrl.ToString());
                Assert.Equal("http://placekitten.com/g/64/64", manifest.Metadata.IconUrl.ToString());
                Assert.Equal("http://nuproj.net/LICENSE/", manifest.Metadata.LicenseUrl.ToString());
                Assert.Equal("Copyright © Immo", manifest.Metadata.Copyright);
                Assert.Equal("NuGetPackage", manifest.Metadata.Tags);
                Assert.Equal(true, manifest.Metadata.DevelopmentDependency);

                var expectedFrameworkAssemblies = new[] {
                    new FrameworkAssemblyReference
                    (
                        "Microsoft.Build.Framework",
                        new [] { NuGetFramework.Parse("net45") }
                    )
                };

                var expectedDependencySets = new[] {
                    new PackageDependencyGroup(
                        NuGetFramework.AnyFramework,
                        new [] {
                            new Core.PackageDependency
                            (
                                "NuGet.Core",
                                VersionRange.Parse("2.8.5")
                            )
                        }
                    )
                };

                var expectedReferenceSets = new[] {
                    new PackageReferenceSet
                    (
                        new [] { "NuGet.Core.dll" }
                    )
                };

                var expectedFiles = new[] {
                    new ManifestFile
                    {
                        Source = "Readme.txt",
                        Target = "",
                        Exclude = "",
                    }
                };

                Assert.Equal(expectedFrameworkAssemblies,
                    manifest.Metadata.FrameworkReferences,
                    FrameworkAssemblyReferenceComparer.Instance);

                Assert.Equal(expectedDependencySets,
                    manifest.Metadata.DependencyGroups,
                    PackageDependencyGroupComparer.Instance);

                Assert.Equal(expectedReferenceSets,
                    manifest.Metadata.PackageAssemblyReferences,
                    PackageReferenceSetComparer.Instance);

                Assert.Equal(expectedFiles,
                    manifest.Files,
                    ManifestFileComparer.Instance);
            }
        }

        [Fact]
        public void GenerateNuSpec_PlaceholderDependencies()
        {
            var output = Path.Combine(projectDirectory, "PlaceholderDependencies.nuspec");

            var target = new GenerateNuSpec();
            target.OutputFileName = output;
            target.Id = "PlaceHolderDependenciesTest";
            target.Version = "1.0.0";
            target.Title = "PlaceHolderDependenciesTest";
            target.Authors = "Nuproj";
            target.RequireLicenseAcceptance = true;
            target.Description = "PlaceHolderDependenciesTest";
            target.ReleaseNotes = "Testing";
            target.Summary = "PlaceHolderDependenciesTest";
            target.ProjectUrl = "http://nuproj.net/changes";
            target.IconUrl = "http://placekitten.com/g/128/128";
            target.LicenseUrl = "http://nuproj.net/LICENSE/changes";
            target.Copyright = "Copyright © Testing";
            target.Tags = "PlaceHolderDependenciesTest";
            target.DevelopmentDependency = false;

            target.Dependencies = new[]
            {
                new TaskItem("APackage", new Dictionary<string, string>
                {
                    { Metadata.TargetFramework, "portable-net45+win80" }
                }),

                new TaskItem("_._", new Dictionary<string, string>
                {
                    { Metadata.TargetFramework, "net45" }
                })
            };

            var result = target.Execute();
            Assert.True(result);

            using (var stream = File.OpenRead(output))
            {
                var manifest = Manifest.ReadFrom(stream, false);
                Assert.Equal(target.Id, manifest.Metadata.Id);
                Assert.Equal(target.Version, manifest.Metadata.Version.ToString());
                Assert.Equal(target.Title, manifest.Metadata.Title);
                Assert.Equal(target.Authors, String.Join(",", manifest.Metadata.Authors));
                Assert.Equal(target.RequireLicenseAcceptance, manifest.Metadata.RequireLicenseAcceptance);
                Assert.Equal(target.Description, manifest.Metadata.Description);
                Assert.Equal(target.ReleaseNotes, manifest.Metadata.ReleaseNotes);
                Assert.Equal(target.Summary, manifest.Metadata.Summary);
                Assert.Equal(target.Language, manifest.Metadata.Language);
                Assert.Equal(target.ProjectUrl, manifest.Metadata.ProjectUrl.ToString());
                Assert.Equal(target.IconUrl, manifest.Metadata.IconUrl.ToString());
                Assert.Equal(target.LicenseUrl, manifest.Metadata.LicenseUrl.ToString());
                Assert.Equal(target.Copyright, manifest.Metadata.Copyright);
                Assert.Equal(target.Tags, manifest.Metadata.Tags);
                Assert.Equal(false, manifest.Metadata.DevelopmentDependency);

                //compare dependencies
                Assert.Equal(2, manifest.Metadata.DependencyGroups.Count());

                foreach (var dependencySet in manifest.Metadata.DependencyGroups)
                {
                    if (dependencySet.TargetFramework.GetShortFolderName() == "net45")
                    {
                        Assert.Equal(0, dependencySet.Packages.Count());
                    }
                    else if (dependencySet.TargetFramework.GetShortFolderName() == "portable-net45+win8")
                    {
                        Assert.Equal(1, dependencySet.Packages.Count());
                        Assert.Equal("APackage", dependencySet.Packages.First().Id);
                    }
                    else
                    {
                        // unexpected dependency set
                        Assert.True(false);
                    }
                }
            }
        }

        [Fact]
        public void GenerateNuSpec_NuSpecFileDependency()
        {
            var nuspecFileDependency = Assets.GetScenarioFilePath("GenerateNuSpec_NuSpecFileDependency", "NuGetPackage.nuspec");
            var output = Path.Combine(projectDirectory, "NuSpecFileDependency.nuspec");

            var target = new GenerateNuSpec();
            target.OutputFileName = output;
            target.Id = "Test";
            target.Version = "1.0.0";
            target.Title = "Test";
            target.Authors = "Nuproj";
            target.RequireLicenseAcceptance = true;
            target.Description = "Test description";
            target.ReleaseNotes = "Testing";
            target.Summary = "Test summary";
            target.ProjectUrl = "http://nuget.org/changes";
            target.IconUrl = "http://placekitten.com/g/128/128";
            target.LicenseUrl = "http://nuget.net/LICENSE/changes";
            target.Copyright = "Copyright © Testing";
            target.Tags = "Tags";
            target.DevelopmentDependency = false;

            target.NuSpecFileDependencies = new []
            {
                new TaskItem(nuspecFileDependency)
            };

            var result = target.Execute();
            Assert.True(result);

            using (var stream = File.OpenRead(output))
            {
                var manifest = Manifest.ReadFrom(stream, false);
                Assert.Equal(target.Id, manifest.Metadata.Id);
                Assert.Equal(target.Version, manifest.Metadata.Version.ToString());
                Assert.Equal(target.Title, manifest.Metadata.Title);
                Assert.Equal(target.Authors, String.Join(",", manifest.Metadata.Authors));
                Assert.Equal(target.RequireLicenseAcceptance, manifest.Metadata.RequireLicenseAcceptance);
                Assert.Equal(target.Description, manifest.Metadata.Description);
                Assert.Equal(target.ReleaseNotes, manifest.Metadata.ReleaseNotes);
                Assert.Equal(target.Summary, manifest.Metadata.Summary);
                Assert.Equal(target.Language, manifest.Metadata.Language);
                Assert.Equal(target.ProjectUrl, manifest.Metadata.ProjectUrl.ToString());
                Assert.Equal(target.IconUrl, manifest.Metadata.IconUrl.ToString());
                Assert.Equal(target.LicenseUrl, manifest.Metadata.LicenseUrl.ToString());
                Assert.Equal(target.Copyright, manifest.Metadata.Copyright);
                Assert.Equal(target.Tags, manifest.Metadata.Tags);
                Assert.Equal(false, manifest.Metadata.DevelopmentDependency);

                var expectedFrameworkAssemblies = new[] {
                    new FrameworkAssemblyReference
                    (
                        "Microsoft.Build.Framework",
                        new [] { NuGetFramework.Parse("net45") }
                    )
                };

                var expectedDependencySets = new[] {
                    new PackageDependencyGroup(
                        NuGetFramework.Parse("net45"),
                        new [] {
                            new Core.PackageDependency
                            (
                                "Mono.Addins",
                                VersionRange.Parse("1.2")
                            )
                        }
                    )
                };

                var expectedReferenceSets = new[] {
                    new PackageReferenceSet
                    (
                        new [] { "NuGet.Core.dll" }
                    )
                };

                var expectedFiles = new[] {
                    new ManifestFile
                    {
                        Source = "Library.dll",
                        Target = "lib/net45/Library.dll",
                        Exclude = "",
                    }
                };

                Assert.Equal(expectedFrameworkAssemblies,
                    manifest.Metadata.FrameworkReferences,
                    FrameworkAssemblyReferenceComparer.Instance);

                Assert.Equal(expectedDependencySets,
                    manifest.Metadata.DependencyGroups,
                    PackageDependencyGroupComparer.Instance);

                Assert.Equal(expectedReferenceSets,
                    manifest.Metadata.PackageAssemblyReferences,
                    PackageReferenceSetComparer.Instance);

                Assert.Equal(expectedFiles,
                    manifest.Files,
                    ManifestFileComparer.Instance);
            }
        }

        [Fact]
        public void GenerateNuSpec_DuplicateItems()
        {
            var input = Assets.GetScenarioFilePath("GenerateNuSpec_DuplicateItems", "NuGetPackage.nuspec");

            // Ensure Manifest.Files have a full path.
            Manifest manifest = null;
            using (var stream = File.OpenRead(input))
            {
                manifest = Manifest.ReadFrom(stream, false);
            }

            string fileToAdd = Path.Combine(projectDirectory, "Readme.txt");
            manifest.Files.First().Source = fileToAdd;
            input = Path.Combine(projectDirectory, "NuGetPackage.nuspec");

            Directory.CreateDirectory(projectDirectory);

            using (var file = File.Create(input))
            {
                manifest.Save(file, false);
            }

            var output = Path.Combine(projectDirectory, "DuplicateItems.nuspec");

            var target = new GenerateNuSpec();
            target.OutputFileName = output;
            target.Id = "DuplicateItems";
            target.Version = "1.0.0";
            target.Title = "DuplicateItems";
            target.Authors = "Nuproj";
            target.RequireLicenseAcceptance = true;
            target.Description = "DuplicateItems";
            target.ReleaseNotes = "Testing";
            target.Summary = "DuplicateItems";
            target.ProjectUrl = "http://nuproj.net/changes";
            target.IconUrl = "http://placekitten.com/g/128/128";
            target.LicenseUrl = "http://nuproj.net/LICENSE/changes";
            target.Copyright = "Copyright © Testing";
            target.DevelopmentDependency = false;
            target.TargetFrameworkMoniker = ".NETFramework, Version=v4.5";
            target.NuSpecFileDependencies = new []
            {
                new TaskItem(input)
            };

            target.Dependencies = new[]
            {
                new TaskItem("NuGet.Core", new Dictionary<string, string>
                {
                    { Metadata.TargetFramework, "net45" },
                    { Metadata.Version, "2.8.5" }
                })
            };

            target.References = new []
            {
                new TaskItem("NuGet.Core.dll", new Dictionary<string, string>
                {
                    { Metadata.TargetFramework, "net45" }
                })
            };

            target.FrameworkReferences = new []
            {
                new TaskItem("Microsoft.Build.Framework", new Dictionary<string, string>
                {
                    { Metadata.TargetFramework, "net45" }
                })
            };

            target.Files = new[]
            {
                new TaskItem(fileToAdd, new Dictionary<string, string>
                {
                    {Metadata.FileSource, fileToAdd},
                    {Metadata.FileTarget, "lib/net45"},
                })
            };

            var result = target.Execute();
            Assert.True(result);

            using (var stream = File.OpenRead(output))
            {
                manifest = Manifest.ReadFrom(stream, false);
                Assert.Equal("DuplicateItems", manifest.Metadata.Id);
                Assert.Equal("1.0.0", manifest.Metadata.Version.ToString());

                var expectedFrameworkAssemblies = new[] {
                    new FrameworkAssemblyReference
                    (
                        "Microsoft.Build.Framework",
                        new [] { NuGetFramework.Parse("net45") }
                    )
                };

                var expectedDependencySets = new[] {
                    new PackageDependencyGroup(
                        NuGetFramework.Parse("net45"),
                        new [] {
                            new Core.PackageDependency
                            (
                                "NuGet.Core",
                                VersionRange.Parse("2.8.5")
                            )
                        }
                    )
                };

                var expectedReferenceSets = new[] {
                    new PackageReferenceSet
                    (
                        "net45",
                        new [] { "NuGet.Core.dll" }
                    )
                };

                var expectedFiles = new[] {
                    new ManifestFile
                    {
                        Source = fileToAdd,
                        Target = "lib/net45",
                        Exclude = ""
                    }
                };

                Assert.Equal(expectedFrameworkAssemblies,
                             manifest.Metadata.FrameworkReferences,
                             FrameworkAssemblyReferenceComparer.Instance);

                Assert.Equal(expectedDependencySets,
                             manifest.Metadata.DependencyGroups,
                             PackageDependencyGroupComparer.Instance);

                Assert.Equal(expectedReferenceSets,
                             manifest.Metadata.PackageAssemblyReferences,
                             PackageReferenceSetComparer.Instance);

                Assert.Equal(expectedFiles,
                             manifest.Files,
                             ManifestFileComparer.Instance);
            }
        }

        [Fact]
        public void GenerateNuSpec_MergePackageDependencies()
        {
            string output = Path.Combine(projectDirectory, "ToBeMerged.nuspec");

            var target = new GenerateNuSpec();
            target.OutputFileName = output;
            target.Version = "1.0.0";

            target.Dependencies = new[]
            {
                new TaskItem("A", new Dictionary<string, string>
                {
                    { Metadata.TargetFramework, "net45" },
                    { Metadata.Version, "2.8.5" }
                }),
                new TaskItem("B", new Dictionary<string, string>
                {
                    { Metadata.TargetFramework, "Xamarin.iOS" },
                    { Metadata.Version, "1.2.3" }
                }),
                new TaskItem("C", new Dictionary<string, string>
                {
                    { Metadata.TargetFramework, "wpa81" },
                    { Metadata.Version, "1.2.4" }
                })
            };

            bool result = target.Execute();
            Assert.True(result);

            string input = output;
            output = Path.Combine(projectDirectory, "MergedPackageDependencies.nuspec");

            target = new GenerateNuSpec();
            target.OutputFileName = output;
            target.Id = "MergePackageDependencies";
            target.Version = "1.0.0";
            target.Authors = "Nuproj";
            target.RequireLicenseAcceptance = true;
            target.Description = "MergePackageDependencies";
            target.ReleaseNotes = "Testing";
            target.ProjectUrl = "http://nuproj.net/changes";
            target.IconUrl = "http://placekitten.com/g/128/128";
            target.LicenseUrl = "http://nuproj.net/LICENSE/changes";
            target.Copyright = "Copyright © Testing";
            target.DevelopmentDependency = false;
            target.TargetFrameworkMoniker = "any";
            target.NuSpecFileDependencies = new []
            {
                new TaskItem(input)
            };

            target.Dependencies = new[]
            {
                new TaskItem("A", new Dictionary<string, string>
                {
                    { Metadata.TargetFramework, "net45" },
                    { Metadata.Version, "2.8.5" }
                }),
                new TaskItem("D", new Dictionary<string, string>
                {
                    { Metadata.TargetFramework, "net45" },
                    { Metadata.Version, "1.0.5" }
                }),
                new TaskItem("E", new Dictionary<string, string>
                {
                    { Metadata.TargetFramework, "Xamarin.iOS" },
                    { Metadata.Version, "5.2.0" }
                }),
                new TaskItem("F", new Dictionary<string, string>
                {
                    { Metadata.TargetFramework, "MonoAndroid" },
                    { Metadata.Version, "3.2.1" }
                }),
            };

            result = target.Execute();
            Assert.True(result);

            using (var stream = File.OpenRead(output))
            {
                var manifest = Manifest.ReadFrom(stream, false);

                // .NET 4.5 package dependencies
                var net45Dependencies = manifest.Metadata.DependencyGroups.Single(
                    item => item.TargetFramework.GetShortFolderName() == "net45");

                var package = net45Dependencies.Packages.Single(p => p.Id == "A");
                Assert.Equal("[2.8.5, )", package.VersionRange.ToString());
                package = net45Dependencies.Packages.Single(p => p.Id == "D");
                Assert.Equal("[1.0.5, )", package.VersionRange.ToString());
                Assert.Equal(2, net45Dependencies.Packages.Count());

                // Windows Phone package dependencies
                var windowsPhoneDependencies = manifest.Metadata.DependencyGroups.Single(
                    item => item.TargetFramework.GetShortFolderName() == "wpa81");

                package = windowsPhoneDependencies.Packages.Single(p => p.Id == "C");
                Assert.Equal("[1.2.4, )", package.VersionRange.ToString());
                Assert.Equal(1, windowsPhoneDependencies.Packages.Count());

                // Android package dependencies
                var androidDependencies = manifest.Metadata.DependencyGroups.Single(
                    item => item.TargetFramework.GetShortFolderName() == "monoandroid");

                package = androidDependencies.Packages.Single(p => p.Id == "F");
                Assert.Equal("[3.2.1, )", package.VersionRange.ToString());
                Assert.Equal(1, androidDependencies.Packages.Count());

                // iOS package dependencies
                var iOSDependencies = manifest.Metadata.DependencyGroups.Single(
                    item => item.TargetFramework.GetShortFolderName() == "xamarinios");

                package = iOSDependencies.Packages.Single(p => p.Id == "B");
                Assert.Equal("[1.2.3, )", package.VersionRange.ToString());
                package = iOSDependencies.Packages.Single(p => p.Id == "E");
                Assert.Equal("[5.2.0, )", package.VersionRange.ToString());
                Assert.Equal(2, iOSDependencies.Packages.Count());
 
                Assert.Equal(4, manifest.Metadata.DependencyGroups.Count());
            }
        }

        [Fact]
        public void GenerateNuSpec_MergeFrameworkReferences()
        {
            string output = Path.Combine(projectDirectory, "ToBeMerged.nuspec");

            var target = new GenerateNuSpec();
            target.OutputFileName = output;
            target.Version = "1.0.0";

            target.FrameworkReferences = new[]
            {
                new TaskItem("A", new Dictionary<string, string>
                {
                    { Metadata.TargetFramework, "net45" },
                }),
                new TaskItem("A", new Dictionary<string, string>
                {
                    { Metadata.TargetFramework, "wpa81" },
                }),
                new TaskItem("B", new Dictionary<string, string>
                {
                    { Metadata.TargetFramework, "Xamarin.iOS" },
                }),
            };

            bool result = target.Execute();
            Assert.True(result);

            string input = output;
            output = Path.Combine(projectDirectory, "MergedFrameworkReferences.nuspec");

            target = new GenerateNuSpec();
            target.OutputFileName = output;
            target.Id = "MergeFrameworkReferences";
            target.Version = "1.0.0";
            target.Authors = "Nuproj";
            target.RequireLicenseAcceptance = true;
            target.Description = "MergeFrameworkReferences";
            target.ReleaseNotes = "Testing";
            target.ProjectUrl = "http://nuproj.net/changes";
            target.IconUrl = "http://placekitten.com/g/128/128";
            target.LicenseUrl = "http://nuproj.net/LICENSE/changes";
            target.Copyright = "Copyright © Testing";
            target.DevelopmentDependency = false;
            target.TargetFrameworkMoniker = "any";
            target.NuSpecFileDependencies = new []
            {
                new TaskItem(input)
            };

            target.FrameworkReferences = new[]
            {
                new TaskItem("A", new Dictionary<string, string>
                {
                    { Metadata.TargetFramework, "net45" },
                }),
                new TaskItem("C", new Dictionary<string, string>
                {
                    { Metadata.TargetFramework, "net45" },
                }),
                new TaskItem("D", new Dictionary<string, string>
                {
                    { Metadata.TargetFramework, "Xamarin.iOS" },
                }),
                new TaskItem("A", new Dictionary<string, string>
                {
                    { Metadata.TargetFramework, "MonoAndroid" },
                })
            };

            result = target.Execute();
            Assert.True(result);

            using (var stream = File.OpenRead(output))
            {
                var manifest = Manifest.ReadFrom(stream, false);

                // 'A' framework references
                var frameworkReferences = manifest.Metadata.FrameworkReferences.Where(
                    item => item.AssemblyName == "A");

                Assert.Contains(frameworkReferences,
                                item => "net45" == item.SupportedFrameworks.Single().GetShortFolderName());
                Assert.Contains(frameworkReferences,
                                item => "wpa81" == item.SupportedFrameworks.Single().GetShortFolderName());
                Assert.Contains(frameworkReferences,
                                item => "monoandroid" == item.SupportedFrameworks.Single().GetShortFolderName());
                Assert.Equal(3, frameworkReferences.Count());

                // 'B' framework reference
                var frameworkReference = manifest.Metadata.FrameworkReferences.Single(
                    item => item.AssemblyName == "B");
                Assert.Equal("xamarinios", frameworkReference.SupportedFrameworks.Single().GetShortFolderName());

                // 'C' framework reference
                frameworkReference = manifest.Metadata.FrameworkReferences.Single(
                    item => item.AssemblyName == "C");
                Assert.Equal("net45", frameworkReference.SupportedFrameworks.Single().GetShortFolderName());

                // 'D' framework reference
                frameworkReference = manifest.Metadata.FrameworkReferences.Single(
                    item => item.AssemblyName == "D");
                Assert.Equal("xamarinios", frameworkReference.SupportedFrameworks.Single().GetShortFolderName());

                Assert.Equal(6, manifest.Metadata.FrameworkReferences.Count());
            }
        }

        [Fact]
        public void GenerateNuSpec_MergePackageAssemblyReferences()
        {
            string output = Path.Combine(projectDirectory, "ToBeMerged.nuspec");

            var target = new GenerateNuSpec();
            target.OutputFileName = output;
            target.Version = "1.0.0";

            target.References = new[]
            {
                new TaskItem("A.dll", new Dictionary<string, string>
                {
                    { Metadata.TargetFramework, "net45" },
                }),
                new TaskItem("B.dll", new Dictionary<string, string>
                {
                    { Metadata.TargetFramework, "Xamarin.iOS" },
                }),
                new TaskItem("C.dll", new Dictionary<string, string>
                {
                    { Metadata.TargetFramework, "wpa81" },
                })
            };

            bool result = target.Execute();
            Assert.True(result);

            string input = output;
            output = Path.Combine(projectDirectory, "MergedPackageAssemblyReferences.nuspec");

            target = new GenerateNuSpec();
            target.OutputFileName = output;
            target.Id = "MergePackageAssemblyReferences";
            target.Version = "1.0.0";
            target.Authors = "Nuproj";
            target.RequireLicenseAcceptance = true;
            target.Description = "MergePackageAssemblyReferences";
            target.ReleaseNotes = "Testing";
            target.ProjectUrl = "http://nuproj.net/changes";
            target.IconUrl = "http://placekitten.com/g/128/128";
            target.LicenseUrl = "http://nuproj.net/LICENSE/changes";
            target.Copyright = "Copyright © Testing";
            target.DevelopmentDependency = false;
            target.TargetFrameworkMoniker = "any";
            target.NuSpecFileDependencies = new []
            {
                new TaskItem(input)
            };

            target.References = new[]
            {
                new TaskItem("A.dll", new Dictionary<string, string>
                {
                    { Metadata.TargetFramework, "net45" },
                }),
                new TaskItem("D.dll", new Dictionary<string, string>
                {
                    { Metadata.TargetFramework, "net45" },
                }),
                new TaskItem("E.dll", new Dictionary<string, string>
                {
                    { Metadata.TargetFramework, "Xamarin.iOS" },
                }),
                new TaskItem("F.dll", new Dictionary<string, string>
                {
                    { Metadata.TargetFramework, "MonoAndroid" },
                }),
            };

            result = target.Execute();
            Assert.True(result);

            using (var stream = File.OpenRead(output))
            {
                var manifest = Manifest.ReadFrom(stream, false);

                // .NET 4.5 package assembly references
                var net45References = manifest.Metadata.PackageAssemblyReferences.Single(
                    item => item.TargetFramework.GetShortFolderName() == "net45");

                Assert.Contains(net45References.References, r => r == "A.dll");
                Assert.Contains(net45References.References, r => r == "D.dll");
                Assert.Equal(2, net45References.References.Count());

                // Windows Phone package assembly references
                var windowsPhoneReferences = manifest.Metadata.PackageAssemblyReferences.Single(
                    item => item.TargetFramework.GetShortFolderName() == "wpa81");

                Assert.Contains(windowsPhoneReferences.References, r => r == "C.dll");
                Assert.Equal(1, windowsPhoneReferences.References.Count());

                // Android package assembly references
                var androidReferences = manifest.Metadata.PackageAssemblyReferences.Single(
                    item => item.TargetFramework.GetShortFolderName() == "monoandroid");

                Assert.Contains(androidReferences.References, r => r == "F.dll");
                Assert.Equal(1, androidReferences.References.Count());

                // iOS package assembly references
                var iOSReferences = manifest.Metadata.PackageAssemblyReferences.Single(
                    item => item.TargetFramework.GetShortFolderName() == "xamarinios");

                Assert.Contains(iOSReferences.References, r => r == "B.dll");
                Assert.Contains(iOSReferences.References, r => r == "E.dll");
                Assert.Equal(2, iOSReferences.References.Count());

                Assert.Equal(4, manifest.Metadata.PackageAssemblyReferences.Count());
            }
        }
    }
}