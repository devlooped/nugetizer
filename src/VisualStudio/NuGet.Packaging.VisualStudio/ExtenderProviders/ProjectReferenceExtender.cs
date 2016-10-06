using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;

namespace NuGet.Packaging.VisualStudio.ExtenderProviders
{
	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	public class ProjectReferenceExtender : IDisposable
	{
		const string CategoryName = "NuGet";

		readonly IVsBuildPropertyStorage propertyStorage;
		readonly uint item;
		readonly IExtenderSite site;
		readonly int cookie;

		public ProjectReferenceExtender(IVsHierarchy hierarchy, uint item, IExtenderSite site, int cookie)
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

		[DefaultValue(true)]
		[Description("Specifies whether the referenced project will be included in the package.")]
		[DisplayName("Include in Package")]
		[Category(CategoryName)]
		public bool IncludeInPackage
		{
			get { return propertyStorage.GetItemAttribute(item, nameof(IncludeInPackage), true); }
			set { propertyStorage.SetItemAttribute(item, nameof(IncludeInPackage), value, true); }
		}
	}
}
