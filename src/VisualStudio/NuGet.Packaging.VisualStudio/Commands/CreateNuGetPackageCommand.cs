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
	class CreateNuGetPackageCommand : DynamicCommand
	{
		const string PackTargetName = "Pack";

		readonly ISolutionExplorer solutionExplorer;
		readonly IDialogService dialogService;
		readonly IVsPackageInstaller packageInstaller;
		readonly IVsPackageInstallerServices packageInstallerServices;
		readonly IBuildService buildService;

		[ImportingConstructor]
		public CreateNuGetPackageCommand(
			ISolutionExplorer solutionExplorer,
			IDialogService dialogService,
			IVsPackageInstaller packageInstaller,
			IVsPackageInstallerServices packageInstallerServices,
			IBuildService msBuildService)
			: base(Commands.CreateNuGetPackageCommandId)
		{
			this.solutionExplorer = solutionExplorer;
			this.dialogService = dialogService;
			this.packageInstaller = packageInstaller;
			this.packageInstallerServices = packageInstallerServices;
			this.buildService = msBuildService;
		}

		protected override void Execute()
		{
			if (ActiveProject == null)
				return;

			var project = ActiveProject.As<EnvDTE.Project>();
			var vsBuildPropertyStorage = ActiveProject.AsVsHierarchy() as IVsBuildPropertyStorage;
			if (vsBuildPropertyStorage != null)
			{
				var storage = new BuildPropertyStorage(vsBuildPropertyStorage);
				var viewModel = new PackageMetadataViewModel(storage);

				if (!packageInstallerServices.IsBuildPackagingNuGetInstalled(project))
				{
					// Provide default values for required fields/properties
					viewModel.PackageId = project.Name;
					viewModel.PackageVersion = "1.0.0";
					viewModel.Description = project.Name;
					viewModel.Authors = "MyCompany";
				}

				var view = new PackageMetadataView() { DataContext = viewModel };
				if (dialogService.ShowDialog(view) == true)
				{
					storage.CommitChanges();

					if (!packageInstallerServices.IsBuildPackagingNuGetInstalled(project))
						packageInstaller.InstallBuildPackagingNuget(project);

					buildService.Pack(ActiveProject);
				}
			}
		}

		protected override void CanExecute(OleMenuCommand command) =>
			command.Enabled = command.Visible = CanExecute();

		IProjectNode ActiveProject => solutionExplorer.Solution.ActiveProject;

		bool CanExecute() => KnownUIContexts.SolutionExistsAndNotBuildingAndNotDebuggingContext.IsActive && ActiveProject != null;
	}
}