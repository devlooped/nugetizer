using System.Linq;
using System.ComponentModel.Composition;
using Clide;

namespace NuGet.Packaging.VisualStudio
{
	[Export(typeof(DynamicCommand))]
	class AddPlatformImplementationCommand : DynamicCommand
	{
		readonly ISolutionExplorer solutionExplorer;
		readonly IDialogService dialogService;
		readonly IPlatformProvider platformProvider;

		[ImportingConstructor]
		public AddPlatformImplementationCommand(
			ISolutionExplorer solutionExplorer,
			IPlatformProvider platformProvider,
			IDialogService dialogService)
			: base(Commands.AddPlatformImplementationCommandId)
		{
			this.solutionExplorer = solutionExplorer;
			this.platformProvider = platformProvider;
			this.dialogService = dialogService;
		}

		protected override void Execute()
		{
			var context = new SolutionContext(solutionExplorer);
			context.Initialize(solutionExplorer.Solution.ActiveProject);

			var viewModel = new AddPlatformImplementationViewModel();

			foreach (var platform in platformProvider.GetSupportedPlatforms())
				viewModel.Platforms.Add(platform);

			viewModel.IsSharedProjectEnabled = context.SharedProject == null;

			var view = new AddPlatformImplementationView();
			view.DataContext = viewModel;

			if (dialogService.ShowDialog(view) == true)
			{
				if (context.SharedProject == null && viewModel.UseSharedProject)
				{
					context.SharedProject = solutionExplorer.Solution.UnfoldTemplate(
						Constants.Templates.SharedProject, context.SharedProjectName);

					// Move PCL items to the shared project
					context.SelectedProject.Accept(
						new MoveProjectItemsToProjectVisitor(context.SharedProject));
				}

				if (context.NuGetProject == null)
				{
					context.NuGetProject = solutionExplorer.Solution.UnfoldTemplate(
						Constants.Templates.NuGetPackage, context.NuGetProjectName, Constants.Language);
				}

				foreach (var selectedPlatform in viewModel.Platforms.Where(x => x.IsEnabled && x.IsSelected))
				{
					var projectName = context.GetTargetProjectName(selectedPlatform);
					var project = context.GetProjectNode(projectName);

					if (project == null)
						project = solutionExplorer.Solution.UnfoldTemplate(
							Constants.Templates.GetPlatformTemplate(selectedPlatform.Id),
							projectName);

					if (context.SharedProject != null)
						project.AddReference(context.SharedProject);

					context.NuGetProject.AddReference(project);
				}
			}
		}
	}
}