using System.Linq;
using Microsoft.Build.Execution;
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
		public void when_library_is_packable_then_contains_content_files_in_anylang_tfm_path()
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
		public void when_include_none_in_package_is_false_then_inferred_none_is_not_included()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage",
				IncludeNoneInPackage = "false",
			});

			result.AssertSuccess(output);

			Assert.DoesNotContain(result.Items, item => item.Matches(new
			{
				TargetPath = "none.txt",
			}));
		}

		[Fact]
		public void when_include_none_in_package_is_false_then_explicitly_included_none_is_included()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage",
				IncludeNoneInPackage = "false",
			});

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				TargetPath = "none-with-include-true.txt",
			}));
		}

		[Fact]
		public void when_none_item_has_no_include_in_package_then_it_is_included_by_default()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage",
			});

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				TargetPath = "none.txt",
			}));
		}

		[Fact]
		public void when_none_item_has_include_in_package_true_then_it_is_included()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage",
			});

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				TargetPath = "none-with-include-true.txt",
			}));
		}

		[Fact]
		public void when_none_item_has_kind_then_kind_is_preserved()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage",
			});

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				TargetPath = "none-with-kind.txt",
				PackagePath = @"build\none-with-kind.txt"
			}));
		}

		[Fact]
		public void when_content_item_has_kind_then_kind_is_preserved()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage",
			});

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				TargetPath = "content-with-kind.txt",
				PackagePath = @"build\content-with-kind.txt"
			}));
		}

		[Fact]
		public void when_content_item_has_include_in_package_false_then_it_is_not_included()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage",
			});

			result.AssertSuccess(output);

			Assert.DoesNotContain(result.Items, item => item.Matches(new
			{
				TargetPath = "content-with-include-false.txt",
			}));
		}

		[Fact]
		public void when_content_item_has_include_in_package_true_then_it_is_included()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage",
			});

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				TargetPath = "content-with-include-true.txt",
			}));
		}

		[Fact]
		public void when_include_content_in_package_is_false_then_explicitly_included_content_is_included()
		{
			var result = Builder.BuildScenario(nameof(given_a_library_with_content), new
			{
				PackageId = "ContentPackage",
				IncludeContentInPackage = "false",
			});

			result.AssertSuccess(output);

			Assert.Contains(result.Items, item => item.Matches(new
			{
				TargetPath = "content-with-include-true.txt",
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
	}
}
