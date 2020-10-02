using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NuGetizer.Tasks;
using Xunit;
using Xunit.Abstractions;
using Metadata = System.Collections.Generic.Dictionary<string, string>;

namespace NuGetizer
{
    public class AssignPackagePathTests
    {
        static ITaskItem[] kinds;

        static ITaskItem[] Kinds
            => kinds ??= new Project(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NuGetizer.props"), null, null, new ProjectCollection())
                .GetItems("PackFolderKind")
                .Select(item => new TaskItem(item.EvaluatedInclude, item.Metadata.ToDictionary(meta => meta.Name, meta => meta.UnevaluatedValue)))
                .ToArray();

        ITestOutputHelper output;
        MockBuildEngine engine;

        public AssignPackagePathTests(ITestOutputHelper output)
        {
            this.output = output;
            engine = new MockBuildEngine(output);
        }

        [Fact]
        public void when_file_has_no_kind_then_logs_error_code()
        {
            var task = new AssignPackagePath
            {
                BuildEngine = engine,
                KnownFolders = Kinds,
                Files = new ITaskItem[]
                {
                    new TaskItem("library.dll", new Metadata
                    {
                        { "PackageId", "A" },
                        { "TargetFrameworkMoniker", ".NETFramework,Version=v4.5" }
                    })
                }
            };

            Assert.False(task.Execute());
            Assert.Equal(nameof(ThisAssembly.Strings.ErrorCode.NG0010), engine.LoggedErrorEvents[0].Code);
        }

        [Fact]
        public void when_file_has_no_kind_and_no_framework_specific_then_it_is_not_assigned_target_framework()
        {
            var task = new AssignPackagePath
            {
                BuildEngine = engine,
                KnownFolders = Kinds,
                Files = new ITaskItem[]
                {
                    new TaskItem("library.dll", new Metadata
                    {
                        { "PackagePath", "workbooks\\library.dll" },
                        { "TargetFrameworkMoniker", ".NETFramework,Version=v4.5" }
                    })
                }
            };

            Assert.True(task.Execute());
            Assert.Contains(task.AssignedFiles, item => item.Matches(new
            {
                PackagePath = "workbooks\\library.dll",
                TargetFramework = ""
            }));
        }

        [Fact]
        public void when_file_has_no_kind_and_package_path_and_framework_specific_then_it_is_assigned_target_framework_only()
        {
            var task = new AssignPackagePath
            {
                BuildEngine = engine,
                KnownFolders = Kinds,
                Files = new ITaskItem[]
                {
                    new TaskItem("library.dll", new Metadata
                    {
                        { "PackagePath", "workbooks\\library.dll" },
                        { "TargetFrameworkMoniker", ".NETFramework,Version=v4.5" },
                        { "FrameworkSpecific", "true" }
                    })
                }
            };

            Assert.True(task.Execute());
            Assert.Contains(task.AssignedFiles, item => item.Matches(new
            {
                PackagePath = "workbooks\\library.dll",
                TargetFramework = "net45"
            }));
        }

        [Fact]
        public void assigned_files_contains_all_files()
        {
            var task = new AssignPackagePath
            {
                BuildEngine = engine,
                KnownFolders = Kinds,
                Files = new ITaskItem[]
                {
                    new TaskItem("a.dll", new Metadata
                    {
                        { "PackageId", "A" },
                        { "TargetFrameworkMoniker", ".NETFramework,Version=v4.5" },
                        { "PackFolder", "lib" }
                    }),
                    new TaskItem("a.pdb", new Metadata
                    {
                        { "PackageId", "A" },
                        { "TargetFrameworkMoniker", ".NETFramework,Version=v4.5" },
                        { "PackFolder", "symbols" }
                    })
                }
            };

            Assert.True(task.Execute());

            Assert.Equal(task.Files.Length, task.AssignedFiles.Length);
        }

        [Fact]
        public void when_file_has_no_package_id_then_package_path_is_not_specified()
        {
            var task = new AssignPackagePath
            {
                BuildEngine = engine,
                KnownFolders = Kinds,
                Files = new ITaskItem[]
                {
                    new TaskItem("library.dll", new Metadata
                    {
                        { "TargetFrameworkMoniker", ".NETFramework,Version=v4.5" },
                        { "PackFolder", "lib" }
                    })
                }
            };

            Assert.True(task.Execute());
            Assert.Equal("", task.AssignedFiles[0].GetMetadata(MetadataName.PackagePath));
        }

        [Fact]
        public void when_file_has_no_package_id_but_is_packaging_true_then_package_path_is_specified()
        {
            var task = new AssignPackagePath
            {
                IsPackaging = "true",
                BuildEngine = engine,
                KnownFolders = Kinds,
                Files = new ITaskItem[]
                {
                    new TaskItem("library.dll", new Metadata
                    {
                        { "TargetFrameworkMoniker", ".NETFramework,Version=v4.5" },
                        { "PackFolder", "lib" }
                    })
                }
            };

            Assert.True(task.Execute());
            Assert.Equal(@"lib\net45\library.dll", task.AssignedFiles[0].GetMetadata(MetadataName.PackagePath));
        }

        [Fact]
        public void when_file_has_no_package_id_then_target_framework_is_calculated_anyway()
        {
            var task = new AssignPackagePath
            {
                BuildEngine = engine,
                KnownFolders = Kinds,
                Files = new ITaskItem[]
                {
                    new TaskItem("library.dll", new Metadata
                    {
                        { "TargetFrameworkMoniker", ".NETFramework,Version=v4.5" },
                        { "PackFolder", "lib" }
                    })
                }
            };

            Assert.True(task.Execute());
            Assert.Contains(task.AssignedFiles, item => item.Matches(new
            {
                TargetFramework = "net45"
            }));
        }

        [Fact]
        public void when_file_has_no_package_id_then_package_folder_is_calculated_anyway()
        {
            var task = new AssignPackagePath
            {
                BuildEngine = engine,
                KnownFolders = Kinds,
                Files = new ITaskItem[]
                {
                    new TaskItem("library.dll", new Metadata
                    {
                        { "TargetFrameworkMoniker", ".NETFramework,Version=v4.5" },
                        { "PackFolder", "lib" }
                    })
                }
            };

            Assert.True(task.Execute());
            Assert.Contains(task.AssignedFiles, item => item.Matches(new
            {
                PackageFolder = "lib"
            }));
        }

        [Fact]
        public void when_file_has_no_tfm_then_assigned_file_contains_no_target_framework()
        {
            var task = new AssignPackagePath
            {
                BuildEngine = engine,
                KnownFolders = Kinds,
                Files = new ITaskItem[]
                {
                    new TaskItem("library.dll", new Metadata
                    {
                        { "PackageId", "A" },
                        { "PackFolder", "lib" }
                    })
                }
            };

            Assert.True(task.Execute());
            Assert.Contains(task.AssignedFiles, item => item.Matches(new
            {
                PackageFolder = "lib",
                PackagePath = "lib\\library.dll",
                TargetFramework = ""
            }));
        }

        [Fact]
        public void when_file_has_target_framework_and_tfm_then_existing_value_is_preserved()
        {
            var task = new AssignPackagePath
            {
                BuildEngine = engine,
                KnownFolders = Kinds,
                Files = new ITaskItem[]
                {
                    new TaskItem("library.dll", new Metadata
                    {
                        { "PackageId", "A" },
                        { "PackFolder", "contentFiles" },
                        { "TargetFramework", "any" },
                        { "TargetFrameworkMoniker", "MonoAndroid,Version=v2.5" },
                    })
                }
            };

            Assert.True(task.Execute());

            Assert.Contains(task.AssignedFiles, item => item.Matches(new
            {
                TargetFramework = "any",
            }));
        }

        [Fact]
        public void when_content_file_has_link_then_package_path_is_relative_to_link()
        {
            var task = new AssignPackagePath
            {
                BuildEngine = engine,
                KnownFolders = Kinds,
                Files = new ITaskItem[]
                {
                    new TaskItem(@"..\..\readme.txt", new Metadata
                    {
                        { "Link", @"docs\readme.txt" },
                        { "PackageId", "A" },
                        { "PackFolder", "content" },
                        { "TargetFramework", "any" },
                    })
                }
            };

            Assert.True(task.Execute());

            Assert.Contains(task.AssignedFiles, item => item.Matches(new
            {
                TargetPath = @"docs\readme.txt",
                PackagePath = @"contentFiles\any\any\docs\readme.txt",
            }));
        }

        [Fact]
        public void when_none_file_has_link_then_package_path_is_relative_to_link()
        {
            var task = new AssignPackagePath
            {
                BuildEngine = engine,
                KnownFolders = Kinds,
                Files = new ITaskItem[]
                {
                    new TaskItem(@"..\..\readme.txt", new Metadata
                    {
                        { "Link", @"docs\readme.txt" },
                        { "PackageId", "A" },
                        { "PackFolder", "none" },
                        { "TargetFramework", "any" },
                    })
                }
            };

            Assert.True(task.Execute());

            Assert.Contains(task.AssignedFiles, item => item.Matches(new
            {
                TargetPath = @"docs\readme.txt",
                PackagePath = @"docs\readme.txt",
            }));
        }


        [Fact]
        public void when_content_is_not_framework_specific_then_has_any_lang_and_tfm()
        {
            var task = new AssignPackagePath
            {
                BuildEngine = engine,
                KnownFolders = Kinds,
                Files = new ITaskItem[]
                {
                    new TaskItem("readme.txt", new Metadata
                    {
                        { "PackageId", "A" },
                        { "PackFolder", "content" },
                        { "FrameworkSpecific", "false" },
                        { "TargetFrameworkMoniker", "MonoAndroid,Version=v2.5" },
                    })
                }
            };

            Assert.True(task.Execute());

            Assert.Contains(task.AssignedFiles, item => item.Matches(new
            {
                CodeLanguage = "any",
                TargetFramework = "any",
                PackagePath = @"contentFiles\any\any\readme.txt"
            }));
        }

        // TODO: these all end up in all lowercase, but MonoAndroid, Xamarin.iOS are usually properly 
        // cased in nupkgs out in the wild (i.e. Rx)
        [InlineData(".NETFramework,Version=v4.5", "net45")]
        [InlineData(".NETPortable,Version=v5.0", "portable50")]
        [InlineData("Xamarin.iOS,Version=v1.0", "xamarinios10")]
        // TODO: should somehow we allow targetting monoandroid without the version suffix?
        [InlineData("MonoAndroid,Version=v2.5", "monoandroid25")]
        [Theory]
        public void when_file_has_tfm_then_assigned_file_contains_target_framework(string targetFrameworkMoniker, string expectedTargetFramework)
        {
            var task = new AssignPackagePath
            {
                BuildEngine = engine,
                KnownFolders = Kinds,
                Files = new ITaskItem[]
                {
                    new TaskItem("library.dll", new Metadata
                    {
                        { "PackageId", "A" },
                        { "TargetFrameworkMoniker", targetFrameworkMoniker },
                        { "PackFolder", "lib" }
                    })
                }
            };

            Assert.True(task.Execute());

            Assert.Contains(task.AssignedFiles, item => item.Matches(new
            {
                PackageFolder = "lib",
                PackagePath = $"lib\\{ expectedTargetFramework}\\library.dll",
                TargetFramework = expectedTargetFramework,
            }));
        }

        public static IEnumerable<object[]> GetMappedKnownKinds => Kinds
            // Skip unmapped kinds (i.e. None, Dependency, etc.)
            .Where(kind => !string.IsNullOrEmpty(kind.GetMetadata(MetadataName.PackageFolder)) &&
                // Skip contentFiles from this test since they get a special map that includes the codelang
                kind.GetMetadata(MetadataName.PackageFolder) != "contentFiles")
            .Select(kind => new object[] { kind.ItemSpec, kind.GetMetadata(MetadataName.PackageFolder), kind.GetMetadata(MetadataName.FrameworkSpecific) });

        [MemberData(nameof(GetMappedKnownKinds))]
        [Theory]
        public void when_file_has_known_kind_then_assigned_file_contains_mapped_package_folder(string packageFileKind, string mappedPackageFolder, string frameworkSpecific)
        {
            var task = new AssignPackagePath
            {
                BuildEngine = engine,
                KnownFolders = Kinds,
                Files = new ITaskItem[]
                {
                    new TaskItem("library.dll", new Metadata
                    {
                        { "PackageId", "A" },
                        { "TargetFrameworkMoniker", ".NETFramework,Version=v4.5" },
                        { "PackFolder", packageFileKind }
                    })
                }
            };

            var isFrameworkSpecific = true;
            bool.TryParse(frameworkSpecific, out isFrameworkSpecific);

            Assert.True(task.Execute());
            Assert.Contains(task.AssignedFiles, item => item.Matches(new
            {
                PackageFolder = mappedPackageFolder,
                PackagePath = $"{mappedPackageFolder}{(isFrameworkSpecific ? "\\net45" : "")}\\library.dll",
            }));
        }

        public static IEnumerable<object[]> GetUnmappedKnownKinds => Kinds
            .Where(kind => string.IsNullOrEmpty(kind.GetMetadata(MetadataName.PackageFolder)) &&
                kind.GetMetadata(MetadataName.PackageFolder) != "contentFiles" &&
                kind.ItemSpec != PackFolderKind.None)
            .Select(kind => new object[] { kind.ItemSpec });

        [MemberData(nameof(GetUnmappedKnownKinds))]
        [Theory]
        public void when_file_has_known_kind_with_no_package_folder_then_package_path_is_empty(string packageFileKind)
        {
            var task = new AssignPackagePath
            {
                BuildEngine = engine,
                KnownFolders = Kinds,
                Files = new ITaskItem[]
                {
                    new TaskItem("Foo", new Metadata
                    {
                        { "PackageId", "A" },
                        { "TargetFrameworkMoniker", ".NETFramework,Version=v4.5" },
                        { "PackFolder", packageFileKind }
                    })
                }
            };

            Assert.True(task.Execute());
            Assert.Contains(task.AssignedFiles, item => item.Matches(new
            {
                PackagePath = "",
            }));
        }

        [Fact]
        public void when_file_has_explicit_package_path_then_calculated_package_folder_is_empty_and_preserves_package_path()
        {
            var task = new AssignPackagePath
            {
                BuildEngine = engine,
                KnownFolders = Kinds,
                Files = new ITaskItem[]
                {
                    new TaskItem("readme.txt", new Metadata
                    {
                        { "PackageId", "A" },
                        { "TargetFrameworkMoniker", ".NETFramework,Version=v4.5" },
                        { "PackFolder", "none" },
                        { "PackagePath", "docs\\readme.txt" }
                    })
                }
            };

            Assert.True(task.Execute());
            Assert.Contains(task.AssignedFiles, item => item.Matches(new
            {
                PackageFolder = "",
                PackagePath = "docs\\readme.txt"
            }));
        }

        [InlineData("", "vb", "contentFiles\\vb\\any\\")]
        [InlineData("", "", "contentFiles\\any\\any\\")]
        [InlineData(".NETFramework,Version=v4.5", "cs", "contentFiles\\cs\\net45\\")]
        [InlineData(".NETFramework,Version=v4.5", "", "contentFiles\\any\\net45\\")]
        [Theory]
        public void when_assigning_content_file_then_applies_tfm_and_language(string tfm, string lang, string expectedPath)
        {
            var task = new AssignPackagePath
            {
                BuildEngine = engine,
                KnownFolders = Kinds,
                Files = new ITaskItem[]
                {
                    new TaskItem("Sample.cs", new Metadata
                    {
                        { "PackageId", "A" },
                        { "TargetFrameworkMoniker", tfm },
                        { "PackFolder", "content" },
                        { "CodeLanguage", lang }
                    })
                }
            };

            Assert.True(task.Execute());
            Assert.Equal("contentFiles", task.AssignedFiles[0].GetMetadata(MetadataName.PackageFolder));
            Assert.True(task.AssignedFiles[0].GetMetadata("PackagePath").StartsWith(expectedPath),
                $"'{task.AssignedFiles[0].GetMetadata("PackagePath")}' does not start with expected '{expectedPath}'");
        }

        [Fact]
        public void when_assigning_content_file_with_reserved_dir_then_fails()
        {
            var task = new AssignPackagePath
            {
                BuildEngine = engine,
                KnownFolders = Kinds,
                Files = new ITaskItem[]
                {
                    new TaskItem(@"contentFiles\cs\monodroid\content.cs", new Metadata
                    {
                        { "PackageId", "A" },
                        { "TargetFrameworkMoniker", ".NETFramework,Version=v4.5" },
                        { "TargetPath", @"contentFiles\cs\monodroid\content.cs" },
                        { "PackFolder", "content" },
                    })
                }
            };

            Assert.False(task.Execute());
            Assert.Contains(engine.LoggedErrorEvents, e => e.Code == nameof(ThisAssembly.Strings.ErrorCode.NG0013));
        }

        [Fact]
        public void when_assigning_content_file_with_additional_metadata_then_preserves_metadata()
        {
            var task = new AssignPackagePath
            {
                BuildEngine = engine,
                KnownFolders = Kinds,
                Files = new ITaskItem[]
                {
                    new TaskItem("Sample.cs", new Metadata
                    {
                        { "PackageId", "A" },
                        { "TargetFrameworkMoniker", ".NETFramework,Version=v4.5" },
                        { "PackFolder", "content" },
                        { MetadataName.ContentFile.CodeLanguage, "cs" },
                        { MetadataName.ContentFile.BuildAction, "EmbeddedResource" },
                        { MetadataName.ContentFile.CopyToOutput, "true" },
                        { MetadataName.ContentFile.Flatten, "true" },
                    })
                }
            };

            Assert.True(task.Execute());
            Assert.Contains(task.AssignedFiles, item => item.Matches(new
            {
                BuildAction = "EmbeddedResource",
                CodeLanguage = "cs",
                CopyToOutput = "true",
                Flatten = "true",
            }));
        }

        [Fact]
        public void when_file_has_none_kind_then_assigned_file_has_empty_package_folder_and_relative_package_path()
        {
            var task = new AssignPackagePath
            {
                BuildEngine = engine,
                KnownFolders = Kinds,
                Files = new ITaskItem[]
                {
                    new TaskItem(@"content\docs\readme.txt", new Metadata
                    {
                        { "PackageId", "A" },
                        { "TargetFrameworkMoniker", ".NETFramework,Version=v4.5" },
                        { "PackFolder", "none" },
                    })
                }
            };

            Assert.True(task.Execute());
            Assert.Contains(task.AssignedFiles, item => item.Matches(new
            {
                PackageFolder = "",
                PackagePath = @"content\docs\readme.txt",
            }));
        }

        [Fact]
        public void when_file_has_none_kind_with_target_path_then_assigned_file_has_empty_package_folder_with_relative_package_path()
        {
            var task = new AssignPackagePath
            {
                BuildEngine = engine,
                KnownFolders = Kinds,
                Files = new ITaskItem[]
                {
                    new TaskItem("library.dll", new Metadata
                    {
                        { "PackageId", "A" },
                        { "TargetFrameworkMoniker", ".NETFramework,Version=v4.5" },
                        { "PackFolder", "none" },
                        { "TargetPath", "workbook\\library.dll"}
                    })
                }
            };

            Assert.True(task.Execute());
            Assert.Contains(task.AssignedFiles, item => item.Matches(new
            {
                PackageFolder = "",
                PackagePath = @"workbook\library.dll",
            }));
        }

        [Fact]
        public void when_file_has_none_kind_then_assigned_file_has_no_target_framework_in_package_path()
        {
            var task = new AssignPackagePath
            {
                BuildEngine = engine,
                KnownFolders = Kinds,
                Files = new ITaskItem[]
                {
                    new TaskItem("library.dll", new Metadata
                    {
                        { "PackageId", "A" },
                        { "TargetFrameworkMoniker", ".NETFramework,Version=v4.5" },
                        { "PackFolder", "none" }
                    })
                }
            };

            Assert.True(task.Execute());
            Assert.NotEqual("net45", task.AssignedFiles[0].GetMetadata(MetadataName.PackagePath).Split(Path.DirectorySeparatorChar)[0]);
        }

        [InlineData("Build", "build")]
        [InlineData("Runtimes", "runtimes")]
        [InlineData("Workbook", "workbook")]
        [Theory]
        public void when_file_has_inferred_folder_from_kind_then_assigned_file_contains_inferred_package_folder(string packageFileKind, string inferredPackageFolder)
        {
            var task = new AssignPackagePath
            {
                BuildEngine = engine,
                KnownFolders = Kinds,
                Files = new ITaskItem[]
                {
                    new TaskItem("library.dll", new Metadata
                    {
                        { "PackageId", "A" },
                        { "TargetFrameworkMoniker", ".NETFramework,Version=v4.5" },
                        { "PackFolder", packageFileKind }
                    })
                }
            };

            Assert.True(task.Execute());
            Assert.Contains(task.AssignedFiles, item => item.Matches(new
            {
                PackageFolder = inferredPackageFolder,
                PackagePath = $"{inferredPackageFolder}\\library.dll",
            }));
        }

        [Fact]
        public void when_file_has_relative_target_path_without_tfm_then_package_path_has_relative_path()
        {
            var task = new AssignPackagePath
            {
                BuildEngine = engine,
                KnownFolders = Kinds,
                Files = new ITaskItem[]
                {
                    new TaskItem("sdk\\bin\\tool.exe", new Metadata
                    {
                        { "PackageId", "A" },
                        { "PackFolder", "tool" },
                        { "TargetPath", "sdk\\bin\\tool.exe"}
                    })
                }
            };

            Assert.True(task.Execute());
            Assert.Contains(task.AssignedFiles, item => item.Matches(new
            {
                PackagePath = @"tools\sdk\bin\tool.exe",
            }));
        }

        [Fact]
        public void when_tool_has_relative_target_path_with_framework_specific_true_then_package_path_has_relative_path_with_target_framework()
        {
            var task = new AssignPackagePath
            {
                BuildEngine = engine,
                KnownFolders = Kinds,
                Files = new ITaskItem[]
                {
                    new TaskItem("sdk\\bin\\tool.exe", new Metadata
                    {
                        { "PackageId", "A" },
                        { "TargetFrameworkMoniker", ".NETFramework,Version=v4.5" },
                        { "PackFolder", "tool" },
                        { "FrameworkSpecific", "true" },
                        { "TargetPath", "sdk\\bin\\tool.exe"}
                    })
                }
            };

            Assert.True(task.Execute());
            Assert.Contains(task.AssignedFiles, item => item.Matches(new
            {
                PackagePath = @"tools\net45\sdk\bin\tool.exe",
            }));
        }

        [Fact]
        public void when_lib_has_framework_specific_false_then_package_path_does_not_have_target_framework()
        {
            var task = new AssignPackagePath
            {
                BuildEngine = engine,
                KnownFolders = Kinds,
                Files = new ITaskItem[]
                {
                    new TaskItem("console.exe", new Metadata
                    {
                        { "PackageId", "A" },
                        { "TargetFrameworkMoniker", ".NETFramework,Version=v4.5" },
                        { "PackFolder", "lib" },
                        { "FrameworkSpecific", "false" },
                    })
                }
            };

            Assert.True(task.Execute());
            Assert.Contains(task.AssignedFiles, item => item.Matches(new
            {
                PackagePath = @"lib\console.exe",
            }));
        }

        [Fact]
        public void when_file_is_not_framework_specific_then_it_is_not_assigned_target_framework()
        {
            var task = new AssignPackagePath
            {
                BuildEngine = engine,
                KnownFolders = Kinds,
                Files = new ITaskItem[]
                {
                    new TaskItem("tools\\foo.exe", new Metadata
                    {
                        { "PackageId", "A" },
                        { "TargetFrameworkMoniker", ".NETFramework,Version=v4.5" },
                        { "PackFolder", "tools" },
                    })
                }
            };

            Assert.True(task.Execute());
            Assert.Contains(task.AssignedFiles, item => item.Matches(new
            {
                PackagePath = @"tools\foo.exe",
                TargetFramework = "",
            }));
        }
    }
}
