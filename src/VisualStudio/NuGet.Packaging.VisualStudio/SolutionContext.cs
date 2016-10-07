using Clide;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NuGet.Packaging.VisualStudio
{
	class SolutionContext
	{
		readonly ISolutionExplorer solutionExplorer;

		public SolutionContext(ISolutionExplorer solutionExplorer)
		{
			this.solutionExplorer = solutionExplorer;
		}

		public void Initialize(IProjectNode baseProject)
		{
			SelectedProject = baseProject;

			BaseProjectName = SelectedProject.Name;

			var projects = solutionExplorer.Solution.FindProjects().ToList();

			// TODO
			// Rather than hardcoding a convention for the shared project name, 
			// is there a way we can instead annotate it with some MSBuild property 
			// that will flag it as "the one" we used before? 
			// This would allow users to rename/move the project freely, 
			// which would make the feature more resilient.
			SharedProjectName = BaseProjectName + "." + Constants.Suffixes.SharedProject;
			SharedProject = GetProjectNode(projects, SharedProjectName);

			NuGetProjectName = BaseProjectName + "." + Constants.Suffixes.NuGetPackage;
			NuGetProject = GetProjectNode(projects, NuGetProjectName);
		}

		public IProjectNode SelectedProject { get; set; }

		public string BaseProjectName { get; set; }

		public string SharedProjectName { get; set; }

		public IProjectNode SharedProject { get; set; }

		public string NuGetProjectName { get; set; }

		public IProjectNode NuGetProject { get; set; }

		IProjectNode GetProjectNode(IEnumerable<IProjectNode> projects, string projectName) =>
			projects.FirstOrDefault(x => x.Name == projectName);

		public IProjectNode GetProjectNode(string projectName) =>
			GetProjectNode(solutionExplorer.Solution.FindProjects(), projectName);

		public IProjectNode GetProjectNode(PlatformViewModel platform) =>	
			GetProjectNode(solutionExplorer.Solution.FindProjects(), GetTargetProjectName(platform));

		public string GetTargetProjectName(PlatformViewModel platform) =>
			BaseProjectName + "." + Constants.Suffixes.GetSuffixForPlatform(platform.Id);
	}
}
