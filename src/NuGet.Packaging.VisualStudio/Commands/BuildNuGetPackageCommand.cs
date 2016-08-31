using Clide;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Composition;

namespace NuGet.Packaging.VisualStudio
{
	[Export(typeof(DynamicCommand))]
	class BuildNuGetPackageCommand : DynamicCommand
	{
		readonly ISolutionExplorer solutionExplorer;
		readonly IDialogService dialogService;

		[ImportingConstructor]
		public BuildNuGetPackageCommand(ISolutionExplorer solutionExplorer, IDialogService dialogService)
			: base(Commands.BuildNuGetPackageCommandId)
		{
			this.solutionExplorer = solutionExplorer;
			this.dialogService = dialogService;
		}

		protected override void Execute()
		{
			var selectedProject = solutionExplorer.Solution.ActiveProject;

			var buildNuGetPackage = true;
			var openNuSpecPropertyPage = false;

			if (!selectedProject.Supports(NuProjCapabilities.NuProj))
				buildNuGetPackage = openNuSpecPropertyPage =
					dialogService.ShowConfirmationMessage(Resources.AddNuProjToLibrary);

			if (buildNuGetPackage)
				selectedProject.BuildNuGetPackage();

			if (openNuSpecPropertyPage)
				selectedProject.OpenNuSpecPropertyPage();
		}
	}
}