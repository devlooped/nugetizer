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
			var project = ActiveProject.As<EnvDTE.Project>();
			var vsBuildPropertyStorage = ActiveProject.AsVsHierarchy() as IVsBuildPropertyStorage;
			if (vsBuildPropertyStorage != null)
			{
				var storage = new BuildPropertyStorage(vsBuildPropertyStorage);
				var viewModel = new PackageMetadataViewModel(storage);

				if (!IsBuildPackagingNuGetInstalled(project))
				{
					// Provide default values for required fields/properties
					viewModel.PackageId = project.Name;
					viewModel.Description = project.Name;
					viewModel.Authors = "MyCompany";
				}

				var view = new PackageMetadataView() { DataContext = viewModel };
				if (dialogService.ShowDialog(view) == true)
				{
					storage.CommitChanges();

					if (!IsBuildPackagingNuGetInstalled(project))
						InstallBuildPackagingNuget(project);

					buildService.Pack(ActiveProject);
				}
			}
		}

		protected override void CanExecute(OleMenuCommand command) =>
			command.Enabled = command.Visible = CanExecute();

		IProjectNode ActiveProject => solutionExplorer.Solution.ActiveProject;

		bool CanExecute() =>
			KnownUIContexts.SolutionExistsAndNotBuildingAndNotDebuggingContext.IsActive &&
			!ActiveProject.Supports(Constants.NuProjCapability); // For NuProj projects the built-in Build command should be used

		bool IsBuildPackagingNuGetInstalled(Project project) =>
			packageInstallerServices.IsPackageInstalled(project, Constants.NuGet.BuildPackagingId);

		void InstallBuildPackagingNuget(Project project) =>
			packageInstaller.InstallPackagesFromVSExtensionRepository(
				Constants.NuGet.RepositoryId,
				false,
				true,
				project,
				new Dictionary<string, string>
				{
					{ Constants.NuGet.BuildPackagingId , Constants.NuGet.BuildPackagingVersion }
				});
	}
}