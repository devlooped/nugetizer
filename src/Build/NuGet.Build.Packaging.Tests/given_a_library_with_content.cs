using Xunit;
using Xunit.Abstractions;

namespace NuGet.Build.Packaging
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
				IncludeContentInPackage = "false"
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

		#region Content scenarios

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
				IncludeContentInPackage = "false"
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
				PackageId = "ContentPackage"
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
				PackagePath = @"contentFiles\any\monoandroid51\content-with-include-true.txt",
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
		public void none_no_kind_neither_include_in_package_is_not_included_by_default()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage",
				IncludeNoneInPackage = "true"
			});

			result.AssertSuccess(output);

			Assert.DoesNotContain(result.Items, item => item.Matches(new
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
