using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Packaging.VisualStudio
{
	[PartCreationPolicy(CreationPolicy.Shared)]
	class ExportProvider
	{
		public const string Dte = "NuGet.Packaging.VisualStudio.Dte";
		public const string DteSolution2 = "NuGet.Packaging.VisualStudio.DteSolution2";

		Lazy<Solution2> solution;
		Lazy<DTE> dte;

		[ImportingConstructor]
		public ExportProvider([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
		{
			dte = new Lazy<DTE>(() => (DTE)serviceProvider.GetService(typeof(DTE)));
			solution = new Lazy<Solution2>(() =>
				 (Solution2)DteExport.Solution);
		}

		[Export(DteSolution2)]
		Solution2 DteSolution2Export => solution.Value;

		[Export(Dte)]
		DTE DteExport => dte.Value;
	}
}
