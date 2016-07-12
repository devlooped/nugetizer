using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Composition;

namespace NuGet.Packaging.VisualStudio
{
	[Export(typeof(DynamicCommand))]
	class BuildNuGetPackageCommand : DynamicCommand
	{
		readonly ISelectionService selectionService;
		readonly IDialogService dialogService;

		[ImportingConstructor]
		public BuildNuGetPackageCommand(ISelectionService selectionService, IDialogService dialogService)
			: base(Commands.BuildNuGetPackageCommandId)
		{
			this.selectionService = selectionService;
			this.dialogService = dialogService;
		}

		protected override void Execute()
		{
			var selectedProject = selectionService.SelectedProject;

			var buildNuGetPackage = true;
			var openNuSpecPropertyPage = false;

			if (!selectedProject.IsNuProjProject)
				buildNuGetPackage = openNuSpecPropertyPage =
					dialogService.ShowConfirmationMessage(Resources.AddNuProjToLibrary);

			if (buildNuGetPackage)
				selectedProject.BuildNuGetPackage();

			if (openNuSpecPropertyPage)
				selectedProject.OpenNuSpecPropertyPage();
		}
	}
}