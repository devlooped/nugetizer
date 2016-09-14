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

namespace NuGet.Packaging
{
	public class AssignPackagePathTests
	{
		static readonly ITaskItem[] kinds;
		ITestOutputHelper output;
		MockBuildEngine engine;

		static AssignPackagePathTests()
		{
			kinds = new Project(Path.Combine(ModuleInitializer.BaseDirectory, "NuGet.Packaging.props"), null, null, new ProjectCollection())
				.GetItems("PackageFileKind")
				.Select(item => new TaskItem(item.UnevaluatedInclude, item.Metadata.ToDictionary(meta => meta.Name, meta => meta.UnevaluatedValue)))
				.ToArray();
		}

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
				Kinds = kinds,
				Files = new ITaskItem[]
				{
					new TaskItem("library.dll", new Dictionary<string,string>
					{
						{ "TargetFrameworkMoniker", ".NETFramework,Version=v4.5" }
					})
				}
			};

			Assert.False(task.Execute());
			Assert.Equal(engine.LoggedErrorEvents[0].Code, nameof(ErrorCode.NP0010));
		}

		[Fact]
		public void assigned_files_contains_all_files()
		{
			var task = new AssignPackagePath
			{
				BuildEngine = engine,
				Kinds = kinds,
				Files = new ITaskItem[] 
				{
					new TaskItem("a.dll", new Dictionary<string,string>
					{
						{ "TargetFrameworkMoniker", ".NETFramework,Version=v4.5" },
						{ "Kind", "Lib" }
					}),
					new TaskItem("a.pdb", new Dictionary<string,string>
					{
						{ "TargetFrameworkMoniker", ".NETFramework,Version=v4.5" },
						{ "Kind", "Symbols" }
					})
				}
			};

			Assert.True(task.Execute());

			Assert.Equal(task.Files.Length, task.AssignedFiles.Length);
		}

		[Fact]
		public void when_file_has_no_tfm_then_assigned_file_contains_no_target_framework()
		{
			var task = new AssignPackagePath
			{
				BuildEngine = engine,
				Kinds = kinds,
				Files = new ITaskItem[]
				{
					new TaskItem("library.dll", new Dictionary<string,string>
					{
						{ "Kind", "Lib" }
					})
				}
			};

			Assert.True(task.Execute());
			Assert.Equal("lib", task.AssignedFiles[0].GetMetadata(MetadataName.PackageFolder));
			Assert.Equal("lib\\library.dll", task.AssignedFiles[0].GetMetadata(MetadataName.PackagePath));
			Assert.Equal("", task.AssignedFiles[0].GetMetadata(MetadataName.TargetFramework));
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
				Kinds = kinds,
				Files = new ITaskItem[]
				{
					new TaskItem("library.dll", new Dictionary<string,string>
					{
						{ "TargetFrameworkMoniker", targetFrameworkMoniker },
						{ "Kind", "Lib" }
					})
				}
			};

			Assert.True(task.Execute());
			Assert.Equal("lib", task.AssignedFiles[0].GetMetadata(MetadataName.PackageFolder));
			Assert.Equal($"lib\\{expectedTargetFramework}\\library.dll", task.AssignedFiles[0].GetMetadata(MetadataName.PackagePath));
			Assert.Equal(expectedTargetFramework, task.AssignedFiles[0].GetMetadata(MetadataName.TargetFramework));
		}

		public static IEnumerable<object[]> GetKnownKinds => kinds
			.Where(kind => kind.ItemSpec != "None")
			.Select(kind => new object[] { kind.ItemSpec, kind.GetMetadata("PackageFolder") });

		[MemberData("GetKnownKinds")]
		[Theory]
		public void when_file_has_known_kind_then_assigned_file_contains_mapped_package_folder(string packageFileKind, string mappedPackageFolder)
		{
			var task = new AssignPackagePath
			{
				BuildEngine = engine,
				Kinds = kinds,
				Files = new ITaskItem[]
				{
					new TaskItem("library.dll", new Dictionary<string,string>
					{
						{ "TargetFrameworkMoniker", ".NETFramework,Version=v4.5" },
						{ "Kind", packageFileKind }
					})
				}
			};

			Assert.True(task.Execute());
			Assert.Equal(mappedPackageFolder, task.AssignedFiles[0].GetMetadata(MetadataName.PackageFolder));
			Assert.Equal($"{mappedPackageFolder}\\net45\\library.dll", task.AssignedFiles[0].GetMetadata(MetadataName.PackagePath));
		}

		[Fact]
		public void when_file_has_none_kind_then_assigned_file_has_empty_package_folder()
		{
			var task = new AssignPackagePath
			{
				BuildEngine = engine,
				Kinds = kinds,
				Files = new ITaskItem[]
				{
					new TaskItem("readme.txt", new Dictionary<string,string>
					{
						{ "TargetFrameworkMoniker", ".NETFramework,Version=v4.5" },
						{ "Kind", "None" }
					})
				}
			};

			Assert.True(task.Execute());
			Assert.Empty(task.AssignedFiles[0].GetMetadata(MetadataName.PackageFolder));
			Assert.Equal(task.AssignedFiles[0].GetMetadata("PackagePath"), "readme.txt");
		}

		[Fact]
		public void when_file_has_none_kind_with_target_path_then_assigned_file_has_empty_package_folder_with_relative_package_path()
		{
			var task = new AssignPackagePath
			{
				BuildEngine = engine,
				Kinds = kinds,
				Files = new ITaskItem[]
				{
					new TaskItem("library.dll", new Dictionary<string,string>
					{
						{ "TargetFrameworkMoniker", ".NETFramework,Version=v4.5" },
						{ "Kind", "None" },
						{ "TargetPath", "workbook\\library.dll"}
					})
				}
			};

			Assert.True(task.Execute());
			Assert.Empty(task.AssignedFiles[0].GetMetadata(MetadataName.PackageFolder));
			Assert.Equal(task.AssignedFiles[0].GetMetadata("PackagePath"), @"workbook\library.dll");
		}

		[Fact]
		public void when_file_has_none_kind_then_assigned_file_has_no_target_framework_in_package_path()
		{
			var task = new AssignPackagePath
			{
				BuildEngine = engine,
				Kinds = kinds,
				Files = new ITaskItem[]
				{
					new TaskItem("library.dll", new Dictionary<string,string>
					{
						{ "TargetFrameworkMoniker", ".NETFramework,Version=v4.5" },
						{ "Kind", "None" }
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
				Kinds = kinds,
				Files = new ITaskItem[]
				{
					new TaskItem("library.dll", new Dictionary<string,string>
					{
						{ "TargetFrameworkMoniker", ".NETFramework,Version=v4.5" },
						{ "Kind", packageFileKind }
					})
				}
			};

			Assert.True(task.Execute());
			Assert.Equal(inferredPackageFolder, task.AssignedFiles[0].GetMetadata(MetadataName.PackageFolder));
			Assert.Equal($"{inferredPackageFolder}\\net45\\library.dll", task.AssignedFiles[0].GetMetadata(MetadataName.PackagePath));
		}

		[Fact]
		public void when_file_has_relative_target_path_without_tfm_then_package_path_has_relative_path()
		{
			var task = new AssignPackagePath
			{
				BuildEngine = engine,
				Kinds = kinds,
				Files = new ITaskItem[]
				{
					new TaskItem("sdk\\bin\\tool.exe", new Dictionary<string,string>
					{
						{ "Kind", "Tool" },
						{ "TargetPath", "sdk\\bin\\tool.exe"}
					})
				}
			};

			Assert.True(task.Execute());
			Assert.Equal(task.AssignedFiles[0].GetMetadata("PackagePath"), @"tools\sdk\bin\tool.exe");
		}

		[Fact]
		public void when_file_has_relative_target_path_with_tfm_then_package_path_has_relative_path_with_target_framework()
		{
			var task = new AssignPackagePath
			{
				BuildEngine = engine,
				Kinds = kinds,
				Files = new ITaskItem[]
				{
					new TaskItem("sdk\\bin\\tool.exe", new Dictionary<string,string>
					{
						{ "TargetFrameworkMoniker", ".NETFramework,Version=v4.5" },
						{ "Kind", "Tool" },
						{ "TargetPath", "sdk\\bin\\tool.exe"}
					})
				}
			};

			Assert.True(task.Execute());
			Assert.Equal(task.AssignedFiles[0].GetMetadata("PackagePath"), @"tools\net45\sdk\bin\tool.exe");
		}

	}
}
