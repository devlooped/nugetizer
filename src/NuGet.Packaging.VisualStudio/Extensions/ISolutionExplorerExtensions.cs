using Clide;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clide
{
	static class ISolutionExplorerExtensions
	{
		public static IProjectNode GetSelectedProject(this ISolutionExplorer solutionExplorer)
		{
			return solutionExplorer.SelectedNodes.OfType<IProjectNode>().FirstOrDefault();
		}
	}
}
