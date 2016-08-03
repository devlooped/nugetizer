using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Composition;

namespace NuGet.Packaging.VisualStudio
{
	[PartCreationPolicy(CreationPolicy.Shared)]
	class ExportProvider
	{
		Lazy<Solution2> solution;
		Lazy<DTE> dte;

		[ImportingConstructor]
		public ExportProvider([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
		{
			dte = new Lazy<DTE>(() => (DTE)serviceProvider.GetService(typeof(DTE)));
			solution = new Lazy<Solution2>(() =>
				 (Solution2)Dte.Solution);
		}

		[Export(Constants.Contracts.DteSolution2)]
		Solution2 Solution2 => solution.Value;

		[Export(Constants.Contracts.Dte)]
		DTE Dte => dte.Value;
	}
}
