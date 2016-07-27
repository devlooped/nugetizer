using System.Linq;

namespace Clide
{
	static class ISolutionExplorerExtensions
	{
		public static IProjectNode GetSelectedProject(this ISolutionExplorer solutionExplorer) =>
			solutionExplorer.SelectedNodes.OfType<IProjectNode>().FirstOrDefault();
	}
}