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
			if (CanExecute())
			{
				var selectedProject = solutionExplorer.Solution.ActiveProject;

				selectedProject.BuildNuGetPackage();
			}
		}

		protected override void CanExecute(OleMenuCommand command)
		{
			command.Checked = command.Visible = CanExecute();
		}

		bool CanExecute() =>
			solutionExplorer.Solution.ActiveProject.Supports(NuProjCapabilities.NuProj);
	}
}