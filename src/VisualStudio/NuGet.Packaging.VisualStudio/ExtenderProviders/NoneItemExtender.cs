using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;

namespace NuGet.Packaging.VisualStudio.ExtenderProviders
{
	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	public class NoneItemExtender : IDisposable
	{
		const string CategoryName = "NuGet";

		readonly IVsBuildPropertyStorage propertyStorage;
		readonly uint item;
		readonly IExtenderSite site;
		readonly int cookie;

		public NoneItemExtender(IVsHierarchy hierarchy, uint item, IExtenderSite site, int cookie)
		{
			propertyStorage = (IVsBuildPropertyStorage)hierarchy;
			this.item = item;
			this.site = site;
			this.cookie = cookie;
		}

		public void Dispose()
		{
			site.NotifyDelete(cookie);
		}

		[DefaultValue(false)]
		[Description("Specifies whether the file will be included in the package. Supported for None items only.")]
		[DisplayName("Include in Package")]
		[Category(CategoryName)]
		public bool IncludeInPackage
		{
			get { return propertyStorage.GetItemAttribute(item, nameof(IncludeInPackage), false); }
			set { propertyStorage.SetItemAttribute(item, nameof(IncludeInPackage), value, false); }
		}
	}
}
