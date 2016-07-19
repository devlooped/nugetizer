using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;
using EnvDTE80;
using System.IO;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.ComponentModelHost;
using System.ComponentModel.Composition;

namespace NuGet.Packaging.VisualStudio
{
	public class CrossPlatformWizard : IWizard
	{
		IUnfoldTemplateService unfoldTemplateService;
		IUnfoldPlatformTemplateService unfoldPlatformTemplateService;
		IPlatformProvider platformProvider;

		public CrossPlatformWizard()
		{ }

		internal CrossPlatformWizard(
			IUnfoldTemplateService unfoldTemplateService,
			IUnfoldPlatformTemplateService unfoldPlatformTemplateService,
			IPlatformProvider platformProvider)
		{
			this.unfoldTemplateService = unfoldTemplateService;
			this.unfoldPlatformTemplateService = unfoldPlatformTemplateService;
			this.platformProvider = platformProvider;
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
			var baseTargetPath = Path.Combine(
					WizardModel.SolutionDirectory,
					WizardModel.SafeProjectName);

			foreach (var selectedPlatform in ViewModel.Platforms.Where(x => x.IsSelected))
				unfoldPlatformTemplateService.UnfoldTemplate(
					selectedPlatform.Id, baseTargetPath);

			unfoldTemplateService.UnfoldTemplate(
				Constants.Templates.SharedProject,
				baseTargetPath + ".Shared");

			unfoldTemplateService.UnfoldTemplate(
				Constants.Templates.NuGetPackage,
				baseTargetPath + ".Package",
				Constants.Language);
		}

		public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
		{
			SatifyDependencies();

			if (WizardModel == null)
				WizardModel = new CrossPlatformWizardModel();

			WizardModel.ParseParameters(replacementsDictionary);

			if (ViewModel == null)
				ViewModel = new CrossPlatformViewModel();

			foreach (var template in platformProvider.GetSupportedPlatforms())
			{
				ViewModel.Platforms.Add(
					new PlatformViewModel
					{
						DisplayName = template.DisplayName,
						Id = template.Id
					});
			}
		}

		void SatifyDependencies()
		{
			var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));

			if (unfoldPlatformTemplateService == null)
				unfoldPlatformTemplateService = componentModel.DefaultExportProvider.GetExportedValue<IUnfoldPlatformTemplateService>();

			if (unfoldTemplateService == null)
				unfoldTemplateService = componentModel.DefaultExportProvider.GetExportedValue<IUnfoldProjectTemplateService>();

			if (platformProvider == null)
				platformProvider = componentModel.DefaultExportProvider.GetExportedValue<IPlatformProvider>();
		}

		public bool ShouldAddProjectItem(string filePath) => true;
	}
}