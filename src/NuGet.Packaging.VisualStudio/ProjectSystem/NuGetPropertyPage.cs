using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyPageBase;
using System.Runtime.InteropServices;

namespace NuGet.Packaging.VisualStudio
{
	[Guid(Guids.NuGetPropertyPageGuid)]
	class NuGetPropertyPage : PropertyPageBase.PropertyPage
	{
		public override string Title => Resources.NuGetPropertyPageTitle;

		protected override string HelpKeyword => String.Empty;

		protected override IPageView GetNewPageView() => new NuGetPropertyPageView(this);

		protected override IPropertyStore GetNewPropertyStore() => new NuGetPropertyStore();
	}
}
