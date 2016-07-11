using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Flavor;
using Microsoft.VisualStudio.Shell.Interop;
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

		protected override int GetProperty(uint itemId, int propId, out object property)
		{
			switch (propId)
			{
				case (int)__VSHPROPID2.VSHPROPID_CfgPropertyPagesCLSIDList:
					{
						// Get the list from the base class
						ErrorHandler.ThrowOnFailure(base.GetProperty(itemId, propId, out property));

						// Add our NuGet Property Page
						property += ';' + typeof(NuGetPropertyPage).GUID.ToString("B");

						return VSConstants.S_OK;
					}
			}

			return base.GetProperty(itemId, propId, out property);
		}
	}
}
