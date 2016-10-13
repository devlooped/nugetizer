using Clide;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using NuGet.Build.Packaging;
using NuGet.VisualStudio;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace NuGet.Packaging.VisualStudio
{
	[Export(typeof(DynamicCommand))]
	class EditNuGetPackageMetadataCommand : DynamicCommand
	{
		const string PackTargetName = "Pack";

		readonly ISolutionExplorer solutionExplorer;
		readonly IDialogService dialogService;
		readonly IVsPackageInstallerServices packageInstallerServices;
		readonly IMsBuildService msBuildService;

		[ImportingConstructor]
		public EditNuGetPackageMetadataCommand(
			ISolutionExplorer solutionExplorer,
			IDialogService dialogService,
			IVsPackageInstallerServices packageInstallerServices,
			IMsBuildService msBuildService)
			: base(Commands.EditNugetPackageMetadataCommandId)
		{
			this.solutionExplorer = solutionExplorer;
			this.dialogService = dialogService;
			this.packageInstallerServices = packageInstallerServices;
			this.msBuildService = msBuildService;
		}

		protected override void Execute()
		{
			if (CanExecute())
			{
				var view = new PackageMetadataView();

				dialogService.ShowDialog(view);
			}
		}

		protected override void CanExecute(OleMenuCommand command)
		{
			command.Enabled = command.Visible = CanExecute();
		}

		IProjectNode ActiveProject => solutionExplorer.Solution.ActiveProject;

		bool CanExecute() =>
			!ActiveProject.Supports(NuProjCapabilities.NuProj) && // For NuProj projects the built-in Build command should be used
			IsBuildPackagingNuGetInstalled(ActiveProject.As<EnvDTE.Project>());

		bool IsBuildPackagingNuGetInstalled(Project project) =>
			packageInstallerServices.IsPackageInstalled(project, Constants.NuGet.BuildPackagingId);
	}
}