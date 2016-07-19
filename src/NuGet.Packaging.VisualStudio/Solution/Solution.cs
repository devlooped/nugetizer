using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Packaging.VisualStudio
{
	[Export(typeof(ISolution))]
	class Solution : ISolution
	{
		readonly DTE dte;
		readonly IVsHierarchyItemManager hierarchyManager;

		[ImportingConstructor]
		public Solution(
			[Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
			IVsHierarchyItemManager hierarchyManager)
		{
			this.dte = (DTE)serviceProvider.GetService(typeof(DTE));
			this.hierarchyManager = hierarchyManager;
		}

		public string Path => dte.Solution.FileName;

		public IProject SelectedProject
		{
			get
			{
				var selectedProject = dte.SelectedItems.Item(1).Project;
				if (selectedProject != null)
					return selectedProject.AsProject();

				return null;
			}
		}
	}
}
