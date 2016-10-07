using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;
using System.IO;
using Microsoft.VisualStudio.ComponentModelHost;
using Clide;
using Microsoft.VisualStudio.Shell.Interop;
using System.Windows.Interop;
using System.Windows;

namespace NuGet.Packaging.VisualStudio
{
	public class MultiPlatformWizard : IWizard
	{
		IPlatformProvider platformProvider;
		ISolutionExplorer solutionExplorer;
		IVsUIShell uiShell;

		public MultiPlatformWizard()
		{ }

		internal MultiPlatformWizard(
			IPlatformProvider platformProvider,
			ISolutionExplorer solutionExplorer,
			IVsUIShell uiShell)
		{
			this.platformProvider = platformProvider;
			this.solutionExplorer = solutionExplorer;
			this.uiShell = uiShell;
		}

		internal string SafeProjectName { get; set; }

		internal string SolutionDirectory { get; set; }

		internal MultiPlatformViewModel ViewModel { get; set; }

		internal MultiPlatformView View { get; set; }

		// Fix unit testing with this for now
		internal bool ShowDialog { get; set; } = true;

		public void BeforeOpeningFile(ProjectItem projectItem)
		{
		}

		public void ProjectFinishedGenerating(EnvDTE.Project project)
		{
		}

		public void ProjectItemFinishedGenerating(ProjectItem projectItem)
		{
		}

		public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
		{
			SatifyDependencies();

			ParseParameters(replacementsDictionary);

			if (ViewModel == null)
				ViewModel = new MultiPlatformViewModel();

			foreach (var platform in platformProvider.GetSupportedPlatforms())
				ViewModel.Platforms.Add(platform);

			if (ShowDialog)
			{
				if (View == null)
				{
					View = new MultiPlatformView();
					View.DataContext = ViewModel;
					uiShell.SetOwner(View);
				}

				if (!View.ShowDialog().GetValueOrDefault())
					throw new WizardBackoutException();
			}
		}

		public void RunFinished()
		{
			var solutionContext = new SolutionContext(solutionExplorer);
			solutionContext.BaseProjectName = SafeProjectName;

			if (ViewModel.UsePlatformSpecific)
			{
				solutionContext.SharedProject = solutionExplorer.Solution.UnfoldTemplate(
					Constants.Templates.SharedProject,
					SafeProjectName + "." + Constants.Suffixes.SharedProject);

				solutionContext.NuGetProject = solutionExplorer.Solution.UnfoldTemplate(
					Constants.Templates.NuGetPackage,
					SafeProjectName + "." + Constants.Suffixes.NuGetPackage,
					Constants.Language);

				foreach (var selectedPlatform in ViewModel.Platforms.Where(x => x.IsSelected))
				{
					var platformProject = solutionExplorer.Solution.UnfoldTemplate(
						Constants.Templates.GetPlatformTemplate(selectedPlatform.Id),
						solutionContext.GetTargetProjectName(selectedPlatform));

					platformProject.AddReference(solutionContext.SharedProject);
					solutionContext.NuGetProject.AddReference(platformProject);
				}
			}
			else
			{
				solutionContext.SharedProject = solutionExplorer.Solution.UnfoldTemplate(
					Constants.Templates.PortableClassLibrary,
					SafeProjectName);
			}
		}

		void SatifyDependencies()
		{
			if (solutionExplorer == null)
				solutionExplorer = ServiceLocator.Global.GetExport<ISolutionExplorer>();

			if (platformProvider == null)
				platformProvider = ServiceLocator.Global.GetExport<IPlatformProvider>();

			if (uiShell == null)
				uiShell = ServiceLocator.Global.GetService<SVsUIShell, IVsUIShell>();
		}

		public bool ShouldAddProjectItem(string filePath) => true;

		internal void ParseParameters(Dictionary<string, string> replacementsDictionary)
		{
			foreach (var param in replacementsDictionary)
			{
				switch (param.Key)
				{
					case "$safeprojectname$": SafeProjectName = param.Value; break;
					case "$solutiondirectory$": SolutionDirectory = param.Value; break;
				}
			}
		}
	}
}