using Microsoft.VisualStudio.TemplateWizard;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace NuGet.Packaging.VisualStudio.UnitTests.Wizards
{
	public class CrossPlatformWizardSpec
	{
		Mock<IUnfoldTemplateService> unfoldTemplateService = new Mock<IUnfoldTemplateService>();

		[Fact]
		public void when_wizard_is_started_with_default_values_then_models_are_created()
		{
			var wizard = new CrossPlatformWizard(unfoldTemplateService.Object);

			wizard.RunStarted(null, new Dictionary<string, string>(), WizardRunKind.AsNewProject, null);

			Assert.NotNull(wizard.WizardModel);
			Assert.NotNull(wizard.ViewModel);
		}

		[Fact]
		public void when_wizard_is_started_then_parameters_are_parsed()
		{
			var wizard = new CrossPlatformWizard(unfoldTemplateService.Object);
			wizard.WizardModel = new TestCrossPlatformWizardModel();

			wizard.RunStarted(null, new Dictionary<string, string>(), WizardRunKind.AsNewProject, null);

			Assert.True(((TestCrossPlatformWizardModel)wizard.WizardModel).IsParsed);
		}

		[Fact]
		public void when_wizard_is_started_then_installed_platform_templates_are_added()
		{
			var wizard = new CrossPlatformWizard(unfoldTemplateService.Object);
			wizard.WizardModel = new TestCrossPlatformWizardModel(new[]
			{
				new PlatformTemplate { DisplayName = "Foo", TemplateId = "InstalledFoo" },
				new PlatformTemplate { DisplayName = "Bar", TemplateId = "UninstalledBar" }
			});

			unfoldTemplateService.Setup(x => x.IsTemplateInstalled("InstalledFoo", It.IsAny<string>())).Returns(true);
			unfoldTemplateService.Setup(x => x.IsTemplateInstalled("UninstalledBar", It.IsAny<string>())).Returns(false);

			wizard.RunStarted(null, new Dictionary<string, string>(), WizardRunKind.AsNewProject, null);

			Assert.Equal(1, wizard.ViewModel.Platforms.Count);
			Assert.Equal("Foo", wizard.ViewModel.Platforms.Single().DisplayName);
		}

		[Fact]
		public void when_wizard_is_finished_then_selected_platforms_are_unfolded()
		{
			var wizard = new CrossPlatformWizard(unfoldTemplateService.Object);

			wizard.WizardModel = new TestCrossPlatformWizardModel(new[]
			{
				new PlatformTemplate
				{
					DisplayName = "Xamarin.iOS",
					TemplateId = "Xamarin.iOS.Library",
					Suffix = "iOS"
				},
				new PlatformTemplate
				{
					DisplayName = "Xamarin.Android",
					TemplateId = "Xamarin.Android.Library",
					Suffix = "Android"
				},
			});
			wizard.WizardModel.SolutionDirectory = @"c:\src\MySolution";
			wizard.WizardModel.SafeProjectName = "MySolution";

			wizard.ViewModel = new CrossPlatformViewModel(new[]
			{
				new PlatformViewModel { DisplayName = "Xamarin.iOS", IsSelected = true },
				new PlatformViewModel { DisplayName = "Xamarin.Android", IsSelected = false }
			});

			wizard.RunFinished();

			unfoldTemplateService.Verify(x => x.UnfoldTemplate("Xamarin.iOS.Library", @"c:\src\MySolution\MySolution.iOS", It.IsAny<string>()));
			unfoldTemplateService.Verify(x => x.UnfoldTemplate("Xamarin.iOS.Android", It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		}

		class TestCrossPlatformWizardModel : CrossPlatformWizardModel
		{
			public TestCrossPlatformWizardModel()
			{ }

			public TestCrossPlatformWizardModel(IEnumerable<PlatformTemplate> platformTemplates)
			{
				this.platformTemplates = platformTemplates.ToList();
			}

			public override void ParseParameters(Dictionary<string, string> replacementsDictionary)
			{
				base.ParseParameters(replacementsDictionary);

				IsParsed = true;
			}

			public bool IsParsed { get; set; }
		}
	}
}
