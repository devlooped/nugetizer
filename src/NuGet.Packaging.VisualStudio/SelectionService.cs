using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Packaging.VisualStudio
{
	[Export(typeof(ISelectionService))]
	class SelectionService : ISelectionService
	{
		readonly DTE dte;

		[ImportingConstructor]
		public SelectionService([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
		{
			this.dte = serviceProvider.GetService(typeof(DTE)) as DTE;
		}

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
