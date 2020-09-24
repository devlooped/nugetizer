using System.IO;
using Microsoft.Build.Execution;
using Xunit;
using Xunit.Abstractions;

namespace NuGetizer
{
    public class given_a_library_with_content
	{
		ITestOutputHelper output;

		public given_a_library_with_content(ITestOutputHelper output)
		{
			this.output = output;
		}

		[Fact]
		public void when_library_is_not_packable_then_still_contains_content_files()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content));

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				TargetPath = @"Resources\drawable-hdpi\Icon.png",
			}));
		}

		[Fact]
		public void when_global_include_content_is_false_then_does_not_contain_content_files()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackContent = "false"
			});

			result.AssertSuccess(output);

			Assert.DoesNotContain(result.Items, item => item.Matches(new
			{
				TargetPath = @"Resources\drawable-hdpi\Icon.png",
			}));
		}

		[Fact]
		public void when_global_include_none_is_false_then_does_not_contain_none_copy_files()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				IncludeNoneInPackage = "false"
			});

			result.AssertSuccess(output);

			Assert.DoesNotContain(result.Items, item => item.Matches(new
			{
				TargetPath = @"none-copy.txt",
			}));
		}

		[Fact]
		public void when_removing_inferred_package_file_from_content_then_content_is_not_included()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage",
				RemoveContent = "true",
			});

			result.AssertSuccess(output);

			Assert.DoesNotContain(result.Items, item => item.GetMetadata("PackagePath").StartsWith("contentFiles"));
		}

		[Fact]
		public void linked_package_file_has_relative_package_path()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage"
			});

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"docs\Readme.txt",
			}));
		}

		#region Content scenarios

		[Fact]
		public void content_no_copy_with_relativedir_can_specify_lang_tfm_metadata()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage"
			});

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"contentFiles\any\any\quickstart\any-any.txt",
			}));
			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"contentFiles\cs\any\quickstart\cs-any.txt",
			}));
			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"contentFiles\fs\monoandroid51\quickstart\fs-tfm.txt",
			}));
			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"contentFiles\any\any\quickstart\any-non-tfm.txt",
			}));
		}

		[Fact]
		public void content_no_copy_is_content_files_anylang_tfm_specific()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage"
			});

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"contentFiles\any\monoandroid51\Resources\drawable-hdpi\Icon.png",
			}));
		}

		[Fact]
		public void content_no_copy_is_included_from_source()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage"
			});

			result.AssertSuccess(output);

			var sourcePath = Path.Combine(Path.GetDirectoryName(result.ProjectOrSolutionFile), "content-with-kind.txt");

			Assert.Contains(result.Items, item => item.Matches(new
			{
				FullPath = sourcePath,
			}));
		}

		[Fact]
		public void content_no_copy_with_contentFiles_dir_fails()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage",
				// Includes "contentFiles\cs\monoandroid\content.cs" which fails
				IncludeContentWithReservedRelativeDir = "true"
			});

			Assert.Equal(TargetResultCode.Failure, result.ResultCode);
			Assert.Contains(result.Logger.Errors, e => e.Code == nameof(ThisAssembly.Strings.ErrorCode.NG0013));
		}

		[Fact]
		public void content_copy_include_false_is_not_included()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage"
			});

			result.AssertSuccess(output);

			Assert.DoesNotContain(result.Items, item => item.Matches(new
			{
				TargetPath = "content-copy-with-include-false.txt",
			}));
		}

		[Fact]
		public void content_copy_include_true_is_included_as_lib()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage"
			});

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\monoandroid51\content-copy-with-include-true.txt",
			}));
		}

		[Fact]
		public void content_copy_with_kind_is_included_as_kind()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage"
			});

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"build\content-copy-with-kind.txt",
			}));
		}


		[Fact]
		public void content_copy_is_included_as_lib()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage"
			});

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\monoandroid51\content-copy.txt",
			}));
		}

		[Fact]
		public void content_include_false_is_not_included()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage"
			});

			result.AssertSuccess(output);

			Assert.DoesNotContain(result.Items, item => item.Matches(new
			{
				TargetPath = "content-with-include-false.txt",
			}));
		}

		[Fact]
		public void content_include_true_is_content_files_anylang_tfm_specific()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage"
			});

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"contentFiles\any\monoandroid51\content-with-include-true.txt",
			}));
		}

		[Fact]
		public void content_include_true_is_included_even_if_global_include_contents_is_false()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage",
				PackContent = "false"
			});

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"contentFiles\any\monoandroid51\content-with-include-true.txt",
			}));
		}

		[Fact]
		public void content_with_kind_is_included_as_kind()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage"
			});

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"build\content-with-kind.txt",
			}));
		}


		[Fact]
		public void content_is_content_files_anylang_tfm_specific()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage"
			});

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"contentFiles\any\monoandroid51\content.txt",
			}));
		}

		[Fact]
		public void content_copy_relative_is_included_as_relative_lib()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage"
			});

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\monoandroid51\relative\content-copy.txt",
			}));
		}

		[Fact]
		public void content_copy_relative_kind_is_included_as_relative_kind()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage"
			});

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"build\relative\content-copy-with-kind.txt",
			}));
		}

		#endregion

		#region None scenarios

		[Fact]
		public void none_no_copy_is_specified_relative_path()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage"
			});

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"contentFiles\cs\monoandroid\none.cs",
			}));
		}

		[Fact]
		public void none_no_copy_is_not_included()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage"
			});

			result.AssertSuccess(output);

			Assert.DoesNotContain(result.Items, item => item.Matches(new
			{
				TargetPath = "none.txt",
			}));
		}

		[Fact]
		public void none_copy_include_false_is_not_included()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage"
			});

			result.AssertSuccess(output);

			Assert.DoesNotContain(result.Items, item => item.Matches(new
			{
				TargetPath = "none-copy-with-include-false.txt",
			}));
		}

		[Fact]
		public void none_copy_include_true_is_included_as_lib()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage"
			});

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\monoandroid51\none-copy-with-include-true.txt",
			}));
		}

		[Fact]
		public void none_copy_with_kind_is_included_as_kind()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage"
			});

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"build\none-copy-with-kind.txt",
			}));
		}

		[Fact]
		public void none_copy_is_included_as_lib()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage",
				IncludeNoneInPackage = "true",
			});

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\monoandroid51\none-copy.txt",
			}));
		}

		[Fact]
		public void non_include_false_is_not_included()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage"
			});

			result.AssertSuccess(output);

			Assert.DoesNotContain(result.Items, item => item.Matches(new
			{
				TargetPath = "none-with-include-false.txt",
			}));
		}

		[Fact]
		public void none_include_true_is_included_as_none()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage"
			});

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"none-with-include-true.txt",
			}));
		}

		[Fact]
		public void none_include_true_is_included_even_if_global_include_none_is_false()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage",
				IncludeNoneInPackage = "false"
			});

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"none-with-include-true.txt",
			}));
		}

		[Fact]
		public void none_with_kind_is_included_as_kind()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage"
			});

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"build\none-with-kind.txt",
			}));
		}

		[Fact]
		public void none_with_kind_is_included_from_source()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage"
			});

			result.AssertSuccess(output);

			var sourcePath = Path.Combine(Path.GetDirectoryName(result.ProjectOrSolutionFile), "none-with-kind.txt");

			Assert.Contains(result.Items, item => item.Matches(new
			{
				FullPath = sourcePath,
			}));
		}

		[Fact]
		public void none_no_kind_is_included__as_none()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage",
				IncludeNoneInPackage = "true"
			});

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				TargetPath = @"none.txt",
			}));
		}

		[Fact]
		public void none_copy_relative_is_included_as_relative_lib()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage"
			});

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"lib\monoandroid51\relative\none-copy.txt",
			}));
		}

		[Fact]
		public void none_copy_relative_kind_is_included_as_relative_kind()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage"
			});

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				PackagePath = @"build\relative\none-copy-with-kind.txt",
			}));
		}

		#endregion
	}
}
