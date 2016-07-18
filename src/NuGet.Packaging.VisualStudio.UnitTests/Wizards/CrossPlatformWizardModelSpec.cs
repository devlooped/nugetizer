using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace NuGet.Packaging.VisualStudio.UnitTests.Wizards
{
	public class CrossPlatformWizardModelSpec
	{
		[Fact]
		public void when_parsing_parameters_then_builtin_parameres_are_parsed()
		{
			var model = new CrossPlatformWizardModel();

			model.ParseParameters(new Dictionary<string, string>
			{
				{"author$", "Microsoft" },
				{"$safeprojectname$", "foo" },
				{"$solutiondirectory$", @"c:\foo" },
			});

			Assert.Equal("foo", model.SafeProjectName);
			Assert.Equal(@"c:\foo", model.SolutionDirectory);
		}

		[Fact]
		public void when_parsing_parameters_then_platform_templates_are_parsed()
		{
			var model = new CrossPlatformWizardModel();

			model.ParseParameters(new Dictionary<string, string>
			{
				{"$PlatformTemplate.iOS$", "Xamarin.iOS.Library|Xamarin.iOS" }
			});

			var platformTemplate = model.PlatformTemplates.FirstOrDefault();

			Assert.NotNull(platformTemplate);
			Assert.Equal(1, model.PlatformTemplates.Count());
			Assert.Equal("Xamarin.iOS.Library", platformTemplate.TemplateId);
		}
	}
}
