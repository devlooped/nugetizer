using System;
using System.Collections.Generic;
using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;

namespace NuGet.Packaging.VisualStudio
{
	public class NewLibraryWizard : IWizard
	{
		Project targetProject;

		public void BeforeOpeningFile(ProjectItem projectItem)
		{
		}

		public void ProjectFinishedGenerating(Project project)
		{
			this.targetProject = project;
		}

		public void ProjectItemFinishedGenerating(ProjectItem projectItem)
		{
		}

		public void RunFinished()
		{
			targetProject.OpenPropertyPage(Guids.NuSpecPropertyPageGuid);
		}

		public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
		{
		}

		public bool ShouldAddProjectItem(string filePath) => true;
	}
}
