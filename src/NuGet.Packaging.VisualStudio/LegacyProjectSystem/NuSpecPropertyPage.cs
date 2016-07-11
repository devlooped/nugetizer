using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyPageBase;
using System.Runtime.InteropServices;

namespace NuGet.Packaging.VisualStudio
{
	[Guid(Guids.NuSpecPropertyPageGuid)]
	class NuSpecPropertyPage : PropertyPageBase.PropertyPage
	{
		public override string Title => Resources.NuSpecPropertyPageTitle;

		protected override string HelpKeyword => String.Empty;

		protected override IPageView GetNewPageView() => new NuSpecPropertyPageView(this);

		protected override IPropertyStore GetNewPropertyStore() => new NuSpecPropertyStore();
	}
}
