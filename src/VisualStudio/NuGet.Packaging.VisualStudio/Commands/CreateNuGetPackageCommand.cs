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
		readonly IMsBuildService msBuildService;

		[ImportingConstructor]
		public CreateNuGetPackageCommand(
			ISolutionExplorer solutionExplorer,
			IDialogService dialogService,
			IVsPackageInstaller packageInstaller,
			IVsPackageInstallerServices packageInstallerServices,
			IMsBuildService msBuildService)
			: base(Commands.CreateNuGetPackageCommandId)
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
				{
					var vsBuildPropertyStorage = ActiveProject.AsVsHierarchy() as IVsBuildPropertyStorage;
					if (vsBuildPropertyStorage != null)
					{
						var storage = new BuildPropertyStorage(vsBuildPropertyStorage);
						var viewModel = new PackageMetadataViewModel(storage)
						{
							// Default values
							PackageId = project.Name,
							PackageVersion = "1.0"
						};

						var view = new PackageMetadataView()
						{
							DataContext = viewModel
						};

						if (dialogService.ShowDialog(view) == true)
						{
							InstallBuildPackagingNuget(project);
							storage.CommitChanges();
						}
						else
						{
							return;
						}
					}
					else
					{
						throw new NotSupportedException("Edit NuGet Package Metadata is not supported for the selected project");
					}
				}

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