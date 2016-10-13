using System;
using System.ComponentModel.Composition;
using Clide;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using NuGet.VisualStudio;

namespace NuGet.Packaging.VisualStudio
{
	[Export(typeof(DynamicCommand))]
	class EditNuGetPackageMetadataCommand : DynamicCommand
	{
		readonly ISolutionExplorer solutionExplorer;
		readonly IDialogService dialogService;
		readonly IVsPackageInstallerServices packageInstallerServices;

		[ImportingConstructor]
		public EditNuGetPackageMetadataCommand(
			ISolutionExplorer solutionExplorer,
			IDialogService dialogService,
			IVsPackageInstallerServices packageInstallerServices)
			: base(Commands.EditNugetPackageMetadataCommandId)
		{
			this.solutionExplorer = solutionExplorer;
			this.dialogService = dialogService;
			this.packageInstallerServices = packageInstallerServices;
		}

		protected override void Execute()
		{
			if (CanExecute())
			{
				var vsBuildPropertyStorage = ActiveProject.AsVsHierarchy() as IVsBuildPropertyStorage;
				if (vsBuildPropertyStorage != null)
				{
					var storage = new BuildPropertyStorage(vsBuildPropertyStorage);
					var viewModel = new PackageMetadataViewModel(storage);

					var view = new PackageMetadataView()
					{
						DataContext = viewModel
					};

					if (dialogService.ShowDialog(view) == true)
						storage.CommitChanges();
				}
				else
				{
					throw new NotSupportedException("Edit NuGet Package Metadata is not supported for the selected project");
				}
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