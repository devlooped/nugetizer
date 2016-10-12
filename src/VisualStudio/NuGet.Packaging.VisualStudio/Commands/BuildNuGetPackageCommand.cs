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
	class BuildNuGetPackageCommand : DynamicCommand
	{
		const string PackTargetName = "Pack";

		readonly ISolutionExplorer solutionExplorer;
		readonly IDialogService dialogService;
		readonly IVsPackageInstaller packageInstaller;
		readonly IVsPackageInstallerServices packageInstallerServices;
		readonly IMsBuildService msBuildService;

		[ImportingConstructor]
		public BuildNuGetPackageCommand(
			ISolutionExplorer solutionExplorer,
			IDialogService dialogService,
			IVsPackageInstaller packageInstaller,
			IVsPackageInstallerServices packageInstallerServices,
			IMsBuildService msBuildService)
			: base(Commands.BuildNuGetPackageCommandId)
		{
			this.solutionExplorer = solutionExplorer;
			this.dialogService = dialogService;
			this.packageInstaller = packageInstaller;
			this.packageInstallerServices = packageInstallerServices;
			this.msBuildService = msBuildService;
		}

		protected override void Execute()
		{
			if (CanExecute())
			{
				var project = ActiveProject.As<EnvDTE.Project>();

				if (!IsBuildPackagingNuGetInstalled(project))
					InstallBuildPackagingNuget(project);

				msBuildService.BeginBuild(project.FullName, PackTargetName);
			}
		}

		protected override void CanExecute(OleMenuCommand command)
		{
			command.Enabled = command.Visible = CanExecute();
		}

		IProjectNode ActiveProject => solutionExplorer.Solution.ActiveProject;

		bool CanExecute() =>
			!ActiveProject.Supports(NuProjCapabilities.NuProj); // For NuProj projects the built-in Build command should be used

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