using Microsoft.VisualStudio.Shell.Flavor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Packaging.VisualStudio
{
	[ComVisible(false)]
	[Guid(Guids.FlavoredProjectTypeGuid)]
	public class NuProjFlavoredProjectFactory : FlavoredProjectFactoryBase
	{
		readonly IServiceProvider package;

		public NuProjFlavoredProjectFactory(IServiceProvider package)
			: base()
		{
			this.package = package;
		}

		protected override object PreCreateForOuter(IntPtr outerProjectIUnknown)
		{
			return new NuProjFlavoredProject(package);
		}
	}
}
