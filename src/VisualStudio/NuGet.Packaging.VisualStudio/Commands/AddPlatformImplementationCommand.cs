using System.Linq;
using System.ComponentModel.Composition;
using Clide;
using System.Windows;
using Microsoft.VisualStudio.Shell;
using NuGet.VisualStudio;

namespace NuGet.Packaging.VisualStudio
{
	[Export(typeof(DynamicCommand))]
	class AddPlatformImplementationCommand : DynamicCommand
	{
		readonly ISolutionExplorer solutionExplorer;
		readonly IDialogService dialogService;
		readonly IPlatformProvider platformProvider;
		readonly IVsPackageInstaller packageInstaller;
		readonly IVsPackageInstallerServices packageInstallerServices;

		[ImportingConstructor]
		public AddPlatformImplementationCommand(
			ISolutionExplorer solutionExplorer,
			IPlatformProvider platformProvider,
			IDialogService dialogService,
			IVsPackageInstaller packageInstaller,
			IVsPackageInstallerServices packageInstallerServices)
			: base(Commands.AddPlatformImplementationCommandId)
		{
			this.solutionExplorer = solutionExplorer;
			this.platformProvider = platformProvider;
			this.dialogService = dialogService;
			this.packageInstaller = packageInstaller;
			this.packageInstallerServices = packageInstallerServices;
		}

		protected override void Execute()
		{
			var context = new SolutionContext(solutionExplorer);
			context.Initialize(solutionExplorer.Solution.ActiveProject);

			var viewModel = new AddPlatformImplementationViewModel();

			foreach (var platform in platformProvider.GetSupportedPlatforms())
			{
				platform.IsEnabled = context.GetProjectNode(platform) == null;
				viewModel.Platforms.Add(platform);
			}

			if (!viewModel.Platforms.Any(x => x.IsEnabled))
			{
				MessageBox.Show(
					"The available platform projects are already present in the current solution. Please select a different library or remove any of the platform projects.",
					"Add Platform Implementation",
					MessageBoxButton.OK,
					MessageBoxImage.Exclamation);

				return;
			}

			viewModel.IsSharedProjectEnabled = context.SharedProject == null;

			var view = new AddPlatformImplementationView();
			view.DataContext = viewModel;

			if (dialogService.ShowDialog(view) == true)
			{
				if (context.SharedProject == null && viewModel.UseSharedProject)
				{
					context.SharedProject = solutionExplorer.Solution.UnfoldTemplate(
						Constants.Templates.SharedProject, context.SharedProjectName);
				}

				if (context.NuGetProject == null)
				{
					context.NuGetProject = solutionExplorer.Solution.UnfoldTemplate(
						Constants.Templates.NuGetPackage, context.NuGetProjectName);
				}

				context.NuGetProject.AddReference(context.SelectedProject);

				EnsureBuildPackagingNugetInstalled(context.SelectedProject);

				foreach (var selectedPlatform in viewModel.Platforms.Where(x => x.IsEnabled && x.IsSelected))
				{
					var projectName = context.GetTargetProjectName(selectedPlatform);
					var project = context.GetProjectNode(projectName);

					if (project == null)
						project = solutionExplorer.Solution.UnfoldTemplate(
							Constants.Templates.GetPlatformTemplate(selectedPlatform.Id),
							projectName);

					EnsureBuildPackagingNugetInstalled(project);

					if (context.SharedProject != null && viewModel.UseSharedProject)
						project.AddReference(context.SharedProject);

					context.NuGetProject.AddReference(project);
				}
			}
		}

		void EnsureBuildPackagingNugetInstalled(IProjectNode project)
		{
			var dteProject = project.As<EnvDTE.Project>();

			if (!packageInstallerServices.IsBuildPackagingNuGetInstalled(dteProject))
				packageInstaller.InstallBuildPackagingNuget(dteProject);
		}

		IProjectNode ActiveProject => solutionExplorer.Solution.ActiveProject;

		protected override void CanExecute(OleMenuCommand command) =>
			command.Enabled = command.Visible = CanExecute();

		bool CanExecute() =>
			KnownUIContexts.SolutionExistsAndNotBuildingAndNotDebuggingContext.IsActive &&
			ActiveProject.Supports(Constants.PortableClassLibraryCapability);
	}
}