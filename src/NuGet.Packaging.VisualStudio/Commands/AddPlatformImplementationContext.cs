using Clide;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NuGet.Packaging.VisualStudio
{
	class AddPlatformImplementationContext
	{
		readonly ISolutionExplorer solutionExplorer;
		List<PlatformViewModel> platforms;

		public AddPlatformImplementationContext(ISolutionExplorer solutionExplorer)
		{
			this.solutionExplorer = solutionExplorer;
		}

		public void Initialize(IPlatformProvider platformProvider)
		{
			SelectedProject = solutionExplorer.GetSelectedProject();

			BasePath = Path.GetDirectoryName(solutionExplorer.Solution.PhysicalPath);
			BaseProjectName = SelectedProject.Name;

			var projects = solutionExplorer.Solution.FindProjects().ToList();

			SharedProjectName = BaseProjectName + "." + Constants.Suffixes.SharedProject;
			SharedProjectPath = Path.Combine(BasePath, SharedProjectName);
			SharedProject = GetProjectNode(projects, SharedProjectName);

			NuGetProjectName = BaseProjectName + "." + Constants.Suffixes.NuGetPackage;
			NuGetProjectPath = Path.Combine(BasePath, NuGetProjectName);
			NuGetProject = GetProjectNode(projects, NuGetProjectName);

			platforms = new List<PlatformViewModel>();
			foreach (var platform in platformProvider.GetSupportedPlatforms())
			{
				platform.ProjectName = BaseProjectName + "." + Constants.Suffixes.GetSuffixForPlatform(platform.Id);
				platform.TargetPath = Path.Combine(BasePath, platform.ProjectName);
				platform.Project = GetProjectNode(projects, platform.ProjectName);

				platforms.Add(platform);
			}
		}

		public IProjectNode SelectedProject { get; set; }

		public IEnumerable<PlatformViewModel> Platforms => platforms;

		public string BasePath { get; set; }

		public string BaseProjectName { get; set; }

		public string SharedProjectName { get; set; }

		public string SharedProjectPath { get; set; }

		public IProjectNode SharedProject { get; set; }

		public string NuGetProjectPath { get; set; }

		public string NuGetProjectName { get; set; }

		public IProjectNode NuGetProject { get; set; }

		IProjectNode GetProjectNode(IEnumerable<IProjectNode> projects, string projectName) =>
			projects.FirstOrDefault(x => x.Name == projectName);
	}
}
