using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;
using System.IO;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.ComponentModelHost;
using Clide;

namespace NuGet.Packaging.VisualStudio
{
	public class CrossPlatformWizard : IWizard
	{
		IPlatformProvider platformProvider;
		ISolutionExplorer solutionExplorer;

		public CrossPlatformWizard()
		{ }

		internal CrossPlatformWizard(
			IPlatformProvider platformProvider,
			ISolutionExplorer solutionExplorer)
		{
			this.platformProvider = platformProvider;
			this.solutionExplorer = solutionExplorer;
		}

		internal CrossPlatformWizardModel WizardModel { get; set; }

		internal CrossPlatformViewModel ViewModel { get; set; }

		public void BeforeOpeningFile(ProjectItem projectItem)
		{
		}

		public void ProjectFinishedGenerating(EnvDTE.Project project)
		{
		}

		public void ProjectItemFinishedGenerating(ProjectItem projectItem)
		{
		}

		public void RunFinished()
		{
			var sharedProject = solutionExplorer.Solution.UnfoldTemplate(
				Constants.Templates.SharedProject,
				WizardModel.SafeProjectName + "." + Constants.Suffixes.SharedProject);

			var nuGetProject = solutionExplorer.Solution.UnfoldTemplate(
				Constants.Templates.NuGetPackage,
				WizardModel.SafeProjectName + "." + Constants.Suffixes.NuGetPackage,
				Constants.Language);

			foreach (var selectedPlatform in ViewModel.Platforms.Where(x => x.IsSelected))
			{
				var platformProject = solutionExplorer.Solution.UnfoldTemplate(
					Constants.Templates.GetPlatformTemplate(selectedPlatform.Id),
					selectedPlatform.ProjectName);

				platformProject.AddReference(sharedProject);
				nuGetProject.AddReference(platformProject);
			}
		}

		public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
		{
			SatifyDependencies();

			if (WizardModel == null)
				WizardModel = new CrossPlatformWizardModel();

			WizardModel.ParseParameters(replacementsDictionary);

			if (ViewModel == null)
				ViewModel = new CrossPlatformViewModel();

			foreach (var platform in platformProvider.GetSupportedPlatforms())
				ViewModel.Platforms.Add(platform);
		}

		void SatifyDependencies()
		{
			var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));

			if (solutionExplorer == null)
				solutionExplorer = componentModel.DefaultExportProvider.GetExportedValue<ISolutionExplorer>();

			if (platformProvider == null)
				platformProvider = componentModel.DefaultExportProvider.GetExportedValue<IPlatformProvider>();
		}

		public bool ShouldAddProjectItem(string filePath) => true;
	}
}