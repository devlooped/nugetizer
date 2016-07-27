using Clide;
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
		Mock<IUnfoldPlatformTemplateService> unfoldPlatformTemplateService = new Mock<IUnfoldPlatformTemplateService>();
		Mock<IPlatformProvider> platformProvider = new Mock<IPlatformProvider>();

		[Fact]
		public void when_wizard_is_started_with_default_values_then_models_are_created()
		{
			var wizard = new CrossPlatformWizard(
				unfoldTemplateService.Object,
				unfoldPlatformTemplateService.Object,
				platformProvider.Object);

			wizard.RunStarted(null, new Dictionary<string, string>(), WizardRunKind.AsNewProject, null);

			Assert.NotNull(wizard.WizardModel);
			Assert.NotNull(wizard.ViewModel);
		}

		[Fact]
		public void when_wizard_is_started_then_supported_platforms_are_added()
		{
			var wizard = new CrossPlatformWizard(
				unfoldTemplateService.Object,
				unfoldPlatformTemplateService.Object,
				platformProvider.Object);

			platformProvider
				.Setup(x => x.GetSupportedPlatforms())
				.Returns(new[]
				{
					new PlatformViewModel { DisplayName = "Foo" }
				});

			wizard.RunStarted(null, new Dictionary<string, string>(), WizardRunKind.AsNewProject, null);

			Assert.Equal(1, wizard.ViewModel.Platforms.Count);
			Assert.Equal("Foo", wizard.ViewModel.Platforms.Single().DisplayName);
		}

		[Fact]
		public void when_wizard_is_finished_then_selected_platforms_are_unfolded()
		{
			var wizard = new CrossPlatformWizard(
				unfoldTemplateService.Object,
				unfoldPlatformTemplateService.Object,
				platformProvider.Object);

			platformProvider
				.Setup(x => x.GetSupportedPlatforms())
				.Returns(new[]
				{
					new PlatformViewModel { Id = "Xamarin.iOS" },
					new PlatformViewModel { Id = "Xamarin.Android" }
				});

			wizard.WizardModel = new CrossPlatformWizardModel();
			wizard.WizardModel.SolutionDirectory = @"c:\src\MySolution";
			wizard.WizardModel.SafeProjectName = "MySolution";

			wizard.ViewModel = new CrossPlatformViewModel(new[]
			{
				new PlatformViewModel { Id = "Xamarin.iOS", IsSelected = true },
				new PlatformViewModel { Id = "Xamarin.Android", IsSelected = false }
			});

			var sharedProject = new Mock<IProjectNode>();
			var nuGetProject = new Mock<IProjectNode>();
			var iosProject = new Mock<IProjectNode>();

			unfoldTemplateService
				.Setup(x => x.UnfoldTemplate(
				   Constants.Templates.SharedProject, It.IsAny<string>(), It.IsAny<string>()))
				.Returns(sharedProject.Object);

			unfoldTemplateService
				.Setup(x => x.UnfoldTemplate(
				   Constants.Templates.NuGetPackage, It.IsAny<string>(), It.IsAny<string>()))
				.Returns(nuGetProject.Object);

			unfoldPlatformTemplateService
				.Setup(x => x.UnfoldTemplate(
				   Constants.Platforms.IOS, It.IsAny<string>()))
				.Returns(iosProject.Object);

			wizard.RunFinished();

			// Verify that the IOS project has been unfolded
			unfoldPlatformTemplateService.Verify(x => x.UnfoldTemplate("Xamarin.iOS", @"c:\src\MySolution\MySolution"));
			// Verify that the Android project has been unfolded
			unfoldPlatformTemplateService.Verify(x => x.UnfoldTemplate("Xamarin.Android", It.IsAny<string>()), Times.Never);

			// iOS project references the shared project
			iosProject.Verify(x => x.AddReference(sharedProject.Object));
			// NuGet project references the iOS project
			nuGetProject.Verify(x => x.AddReference(iosProject.Object));
		}
	}
}
