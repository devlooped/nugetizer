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
		readonly IUnfoldPlatformTemplateService unfoldPlatformTemplateService;
		readonly IUnfoldProjectTemplateService unfoldProjectTemplateService;

		[ImportingConstructor]
		public AddPlatformImplementationCommand(
			ISolutionExplorer solutionExplorer,
			IPlatformProvider platformProvider,
			IUnfoldProjectTemplateService unfoldProjectTemplateService,
			IUnfoldPlatformTemplateService unfoldPlatformTemplateService,
			IDialogService dialogService)
			: base(Commands.AddPlatformImplementationCommandId)
		{
			this.solutionExplorer = solutionExplorer;
			this.platformProvider = platformProvider;
			this.dialogService = dialogService;
			this.unfoldPlatformTemplateService = unfoldPlatformTemplateService;
			this.unfoldProjectTemplateService = unfoldProjectTemplateService;
		}

		protected override void Execute()
		{
			var context = new AddPlatformImplementationContext(solutionExplorer);
			context.Initialize(platformProvider);

			var viewModel = new AddPlatformImplementationViewModel();

			foreach (var platform in context.Platforms)
				viewModel.Platforms.Add(platform);

			viewModel.IsSharedProjectEnabled = context.SharedProject == null;

			var view = new AddPlatformImplementationView();
			view.DataContext = viewModel;

			if (dialogService.ShowDialog(view) == true)
			{
				if (context.SharedProject == null && viewModel.UseSharedProject)
				{
					context.SharedProject = unfoldProjectTemplateService.UnfoldTemplate(
						Constants.Templates.SharedProject, context.SharedProjectPath);

					// Move PCL items to the shared project
					context.SelectedProject.Accept(
						new MoveProjectItemsToProjectVisitor(context.SharedProject));
				}

				if (context.NuGetProject == null)
				{
					context.NuGetProject = unfoldProjectTemplateService.UnfoldTemplate(
						Constants.Templates.NuGetPackage, context.NuGetProjectPath, Constants.Language);
				}

				foreach (var selectedPlatform in viewModel.Platforms.Where(x => x.IsEnabled && x.IsSelected))
				{
					if (selectedPlatform.Project == null)
					{
						selectedPlatform.Project = unfoldPlatformTemplateService.UnfoldTemplate(
							selectedPlatform.Id, selectedPlatform.TargetPath);
					}

					if (context.SharedProject != null)
						selectedPlatform.Project.AddReference(context.SharedProject);

					context.NuGetProject.AddReference(selectedPlatform.Project);
				}
			}
		}
	}
}