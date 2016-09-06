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
		Mock<ISolutionExplorer> solutionExplorer = new Mock<ISolutionExplorer>();
		Mock<IPlatformProvider> platformProvider = new Mock<IPlatformProvider>();

		[Fact]
		public void when_wizard_is_started_with_default_values_then_models_are_created()
		{
			var wizard = new CrossPlatformWizard(
				platformProvider.Object, solutionExplorer.Object);

			wizard.RunStarted(null, new Dictionary<string, string>(), WizardRunKind.AsNewProject, null);

			Assert.NotNull(wizard.WizardModel);
			Assert.NotNull(wizard.ViewModel);
		}

		[Fact]
		public void when_wizard_is_started_then_supported_platforms_are_added()
		{
			var wizard = new CrossPlatformWizard(
				platformProvider.Object, solutionExplorer.Object);

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
				platformProvider.Object, solutionExplorer.Object);

			var solution = new Mock<ISolutionNode>();
			var solutionAsProjectContainer = new Mock<IProjectContainerNode>();

			solution
				.Setup(x => x.As<IProjectContainerNode>())
				.Returns(solutionAsProjectContainer.Object);

			solutionExplorer
				.Setup(x => x.Solution)
				.Returns(solution.Object);

			platformProvider
				.Setup(x => x.GetSupportedPlatforms())
				.Returns(new[]
				{
					new PlatformViewModel { Id = "Xamarin.iOS" },
					new PlatformViewModel { Id = "Xamarin.Android" }
				});

			wizard.WizardModel = new CrossPlatformWizardModel();
			wizard.WizardModel.SolutionDirectory = @"c:\src\App";
			wizard.WizardModel.SafeProjectName = "App";

			wizard.ViewModel = new CrossPlatformViewModel(new[]
			{
				new PlatformViewModel { Id = "Xamarin.iOS", IsSelected = true },
				new PlatformViewModel { Id = "Xamarin.Android", IsSelected = false }
			});

			var sharedProject = new Mock<IProjectNode>();
			var nuGetProject = new Mock<IProjectNode>();
			var nuGetProjectAsRefrenceContainer = new Mock<IReferenceContainerNode>();
			nuGetProject.Setup(x => x.As<IReferenceContainerNode>()).Returns(nuGetProjectAsRefrenceContainer.Object);
			var iosProject = new Mock<IProjectNode>();
			var iosProjectAsReferenceContainer = new Mock<IReferenceContainerNode>();
			iosProject.Setup(x => x.As<IReferenceContainerNode>()).Returns(iosProjectAsReferenceContainer.Object);

			solutionAsProjectContainer
				.Setup(x => x.UnfoldTemplate(
				   Constants.Templates.SharedProject, It.IsAny<string>(), It.IsAny<string>()))
				.Returns(sharedProject.Object);

			solutionAsProjectContainer
				.Setup(x => x.UnfoldTemplate(
				   Constants.Templates.NuGetPackage, It.IsAny<string>(), It.IsAny<string>()))
				.Returns(nuGetProject.Object);

			solutionAsProjectContainer
				.Setup(x => x.UnfoldTemplate(
				   Constants.Templates.IOS, It.IsAny<string>(), It.IsAny<string>()))
				.Returns(iosProject.Object);

			wizard.RunFinished();

			// Verify that the IOS project has been unfolded
			solutionAsProjectContainer.Verify(x => x.UnfoldTemplate("Xamarin.iOS.Library", @"App.iOS", It.IsAny<string>()));
			// Verify that the Android project has been unfolded
			solutionAsProjectContainer.Verify(x => x.UnfoldTemplate("Xamarin.Android.ClassLibrary", It.IsAny<string>(), It.IsAny<string>()), Times.Never);

			// iOS project references the shared project
			iosProjectAsReferenceContainer.Verify(x => x.AddReference(sharedProject.Object));
			// NuGet project references the iOS project
			nuGetProjectAsRefrenceContainer.Verify(x => x.AddReference(iosProject.Object));
		}
	}
}
