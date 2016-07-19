using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Composition;

namespace NuGet.Packaging.VisualStudio
{
	[Export(typeof(DynamicCommand))]
	class BuildNuGetPackageCommand : DynamicCommand
	{
		readonly ISolution solution;
		readonly IDialogService dialogService;

		[ImportingConstructor]
		public BuildNuGetPackageCommand(ISolution solution, IDialogService dialogService)
			: base(Commands.BuildNuGetPackageCommandId)
		{
			this.solution = solution;
			this.dialogService = dialogService;
		}

		protected override void Execute()
		{
			var selectedProject = solution.SelectedProject;

			var buildNuGetPackage = true;
			var openNuSpecPropertyPage = false;

			if (!selectedProject.IsNuProj)
				buildNuGetPackage = openNuSpecPropertyPage =
					dialogService.ShowConfirmationMessage(Resources.AddNuProjToLibrary);

			if (buildNuGetPackage)
				selectedProject.BuildNuGetPackage();

			if (openNuSpecPropertyPage)
				selectedProject.OpenNuSpecPropertyPage();
		}
	}
}