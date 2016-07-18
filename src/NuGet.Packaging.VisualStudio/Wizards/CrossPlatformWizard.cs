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

		public CrossPlatformWizard()
		{ }

		internal CrossPlatformWizard(IUnfoldTemplateService unfoldTemplateService)
		{
			this.unfoldTemplateService = unfoldTemplateService;
		}

		internal CrossPlatformWizardModel WizardModel { get; set; }

		internal CrossPlatformViewModel ViewModel { get; set; }

		public void BeforeOpeningFile(ProjectItem projectItem)
		{
		}

		public void ProjectFinishedGenerating(Project project)
		{
		}

		public void ProjectItemFinishedGenerating(ProjectItem projectItem)
		{
		}

		public void RunFinished()
		{
			foreach (var selectedPlatform in ViewModel.Platforms.Where(x => x.IsSelected))
			{
				var platformTemplate = WizardModel.PlatformTemplates.FirstOrDefault(x =>
						x.DisplayName == selectedPlatform.DisplayName);

				if (platformTemplate != null)
				{
					var targetPlatformPath = Path.Combine(
							WizardModel.SolutionDirectory,
							WizardModel.SafeProjectName + "." + platformTemplate.Suffix);

					unfoldTemplateService.UnfoldTemplate(
						platformTemplate.TemplateId, targetPlatformPath);
				}
			}

			unfoldTemplateService.UnfoldTemplate(
				Constants.NuGetPackageProjectTemplateId,
				Path.Combine(
					WizardModel.SolutionDirectory,
					WizardModel.SafeProjectName + ".Package"),
				Constants.Language);
		}

		public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
		{
			if (unfoldTemplateService == null)
				unfoldTemplateService = GetExportedValue<IUnfoldProjectTemplateService>();

			if (WizardModel == null)
				WizardModel = new CrossPlatformWizardModel();

			WizardModel.ParseParameters(replacementsDictionary);

			if (ViewModel == null)
				ViewModel = new CrossPlatformViewModel();

			foreach (var template in WizardModel.PlatformTemplates
				.Where(x => unfoldTemplateService.IsTemplateInstalled(x.TemplateId)))
			{
				ViewModel.Platforms.Add(
					new PlatformViewModel
					{
						DisplayName = template.DisplayName
					});
			}
		}

		T GetExportedValue<T>()
		{
			var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));

			return componentModel.DefaultExportProvider.GetExportedValue<T>();
		}

		public bool ShouldAddProjectItem(string filePath) => true;
	}
}