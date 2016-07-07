using Microsoft.VisualStudio.Shell.Flavor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Packaging.VisualStudio
{
	class NuProjFlavoredProject : FlavoredProjectBase
	{
		public NuProjFlavoredProject(IServiceProvider serviceProvider)
			: base()
		{
			base.serviceProvider = serviceProvider;
		}

		protected override void SetInnerProject(IntPtr innerIUnknown)
		{
			base.SetInnerProject(innerIUnknown);
		}
	}
}
