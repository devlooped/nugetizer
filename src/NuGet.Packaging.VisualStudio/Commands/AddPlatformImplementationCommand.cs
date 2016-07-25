using Microsoft.VisualStudio.Shell;
using System.Linq;
using System.ComponentModel.Composition;
using System.IO;
using Clide;

namespace NuGet.Packaging.VisualStudio
{
	[Export(typeof(DynamicCommand))]
	class AddPlatformImplementationCommand : DynamicCommand
	{
		readonly ISolutionExplorer solutionExplorer;
		readonly IDialogService dialogService;
		readonly IPlatformProvider platformProvider;
		readonly IUnfoldPlatformTemplateService unfoldTemplateService;
		readonly IUnfoldProjectTemplateService unfoldProjectTemplateService;

		[ImportingConstructor]
		public AddPlatformImplementationCommand(
			ISolutionExplorer solutionExplorer,
			IPlatformProvider platformProvider,
			IUnfoldProjectTemplateService unfoldProjectTemplateService,
			IUnfoldPlatformTemplateService unfoldTemplateService,
			IDialogService dialogService)
			: base(Commands.AddPlatformImplementationCommandId)
		{
			this.solutionExplorer = solutionExplorer;
			this.platformProvider = platformProvider;
			this.dialogService = dialogService;
			this.unfoldTemplateService = unfoldTemplateService;
			this.unfoldProjectTemplateService = unfoldProjectTemplateService;
		}

		protected override void Execute()
		{
			var viewModel = new AddPlatformImplementationViewModel();

			foreach (var platform in platformProvider.GetSupportedPlatforms())
				viewModel.Platforms.Add(platform);

			var view = new AddPlatformImplementationView();
			view.DataContext = viewModel;

			if (dialogService.ShowDialog(view) == true)
			{
				var targetBasePath = Path.Combine(
					Path.GetDirectoryName(solutionExplorer.Solution.PhysicalPath),
					solutionExplorer.GetSelectedProject().Name);

				foreach (var selectedPlatform in viewModel.Platforms.Where(x => x.IsEnabled && x.IsSelected))
					unfoldTemplateService.UnfoldTemplate(selectedPlatform.Id, targetBasePath);

				var targetSharedProjectPath = targetBasePath + ".Shared";
				var targetPackagePath = targetBasePath + ".Package";

				if (viewModel.UseSharedProject && !Directory.Exists(targetSharedProjectPath))
					unfoldProjectTemplateService.UnfoldTemplate(
						Constants.Templates.SharedProject, targetSharedProjectPath);

				if (!Directory.Exists(targetPackagePath))
					unfoldProjectTemplateService.UnfoldTemplate(
						Constants.Templates.NuGetPackage, targetPackagePath, Constants.Language);
			}
		}
	}
}