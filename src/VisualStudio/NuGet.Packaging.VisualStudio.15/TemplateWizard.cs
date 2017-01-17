using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using System.IO;

namespace NuGet.Packaging.VisualStudio
{
	public class TemplateWizard : IWizard

	{
		public void BeforeOpeningFile(ProjectItem projectItem)
		{
		}

		public void ProjectFinishedGenerating(Project project)
		{
			try
			{
				// HACK: For now the NuGet.VisualStudio.TemplateWizard fails to install the packages
				// if the base intermediate output path (obj) directory is not already created
				// So adding this check/fix until the issue can be resolved
				var baseIntermediateOutputPath = Path.Combine(Path.GetDirectoryName(project.FullName), "obj");
				if (!Directory.Exists(baseIntermediateOutputPath))
					Directory.CreateDirectory(baseIntermediateOutputPath);
			}
			catch { }
		}

		public void ProjectItemFinishedGenerating(ProjectItem projectItem)
		{
		}

		public void RunFinished()
		{
		}

		public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
		{
		}

		public bool ShouldAddProjectItem(string filePath) => true;
	}
}
