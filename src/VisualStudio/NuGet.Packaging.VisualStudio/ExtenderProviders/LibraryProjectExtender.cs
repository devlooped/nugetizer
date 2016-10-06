using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace NuGet.Packaging.VisualStudio.ExtenderProviders
{
	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	public class LibraryProjectExtender
	{
		const string CategoryName = "NuGet";
		const string BuildCategoryName = "NuGet Build";

		readonly IVsBuildPropertyStorage propertyStorage;
		readonly IExtenderSite site;
		readonly int cookie;

		internal LibraryProjectExtender(IVsHierarchy hierarchy, IExtenderSite site, int cookie)
		{
			propertyStorage = (IVsBuildPropertyStorage)hierarchy;
			this.site = site;
			this.cookie = cookie;
		}

		public void Dispose()
		{
			site.NotifyDelete(cookie);
		}

		[DefaultValue(true)]
		[Description("Whether to include the primary outputs of this project in the package. Defaults to 'true'.")]
		[DisplayName("Include Outputs")]
		[Category(BuildCategoryName)]
		public bool IncludeOutputsInPackage
		{
			get { return propertyStorage.GetGlobalProperty(nameof(IncludeOutputsInPackage), true); }
			set { propertyStorage.SetGlobalProperty(nameof(IncludeOutputsInPackage), value, true); }
		}

		[DefaultValue(true)]
		[Description("Whether to include symbols in the package if 'Include Outputs' is 'true'. Defaults to 'true' for debug builds, 'false' otherwise.")]
		[DisplayName("Include Symbols")]
		[Category(BuildCategoryName)]
		public bool IncludeSymbolsInPackage
		{
			get { return propertyStorage.GetGlobalProperty(nameof(IncludeSymbolsInPackage), true); }
			set { propertyStorage.SetGlobalProperty(nameof(IncludeSymbolsInPackage), value, true); }
		}

		[DefaultValue(true)]
		[Description("Whether to include the content items in the package. Defaults to 'true'.")]
		[DisplayName("Include Content")]
		[Category(BuildCategoryName)]
		public bool IncludeContentInPackage
		{
			get { return propertyStorage.GetGlobalProperty(nameof(IncludeContentInPackage), true); }
			set { propertyStorage.SetGlobalProperty(nameof(IncludeContentInPackage), value, true); }
		}

		[DefaultValue(true)]
		[Description("Whether to include the referenced framework assemblies as dependencies in the package. Defaults to 'true'.")]
		[DisplayName("Include Framework References")]
		[Category(BuildCategoryName)]
		public bool IncludeFrameworkReferencesInPackage
		{
			get { return propertyStorage.GetGlobalProperty(nameof(IncludeFrameworkReferencesInPackage), true); }
			set { propertyStorage.SetGlobalProperty(nameof(IncludeFrameworkReferencesInPackage), value, true); }
		}

		[DefaultValue("")]
		[Description("The output directory for the generated NuGet package. Defaults to the project's $(OutputPath) property.")]
		[DisplayName("Output Path")]
		[Category(BuildCategoryName)]
		public string PackageOutputPath
		{
			get { return propertyStorage.GetGlobalProperty(nameof(PackageOutputPath), ""); }
			set { propertyStorage.SetGlobalProperty(nameof(PackageOutputPath), value, ""); }
		}

		[DefaultValue(false)]
		[Description("Specifies if the output NuGet package should be generated on build. Only applies to projects that have a package Id set.")]
		[DisplayName("Pack on Build")]
		[Category(BuildCategoryName)]
		public bool PackOnBuild
		{
			get { return propertyStorage.GetGlobalProperty(nameof(PackOnBuild), false); }
			set { propertyStorage.SetGlobalProperty(nameof(PackOnBuild), value, false); }
		}

		[DefaultValue("")]
		[Description("Package identifier.")]
		[DisplayName("Id")]
		[Category(CategoryName)]
		public string PackageId
		{
			get { return propertyStorage.GetGlobalProperty(nameof(PackageId), ""); }
			set { propertyStorage.SetGlobalProperty(nameof(PackageId), value, ""); }
		}

		[DefaultValue("")]
		[Description("Package version.")]
		[DisplayName("Version")]
		[Category(CategoryName)]
		public string PackageVersion
		{
			get { return propertyStorage.GetGlobalProperty(nameof(PackageVersion), ""); }
			set { propertyStorage.SetGlobalProperty(nameof(PackageVersion), value, ""); }
		}

		[DefaultValue(false)]
		[Description("Specifies if the project generates a development dependency package.")]
		[DisplayName("Is Development Dependency")]
		[Category(CategoryName)]
		public bool IsDevelopmentDependency
		{
			get { return propertyStorage.GetGlobalProperty(nameof(IsDevelopmentDependency), false); }
			set { propertyStorage.SetGlobalProperty(nameof(IsDevelopmentDependency), value, false); }
		}

		[DefaultValue("")]
		[Category(CategoryName)]
		public string Authors
		{
			get { return propertyStorage.GetGlobalProperty(nameof(Authors), ""); }
			set { propertyStorage.SetGlobalProperty(nameof(Authors), value, ""); }
		}

		[DefaultValue("")]
		[Category(CategoryName)]
		public string Title
		{
			get { return propertyStorage.GetGlobalProperty(nameof(Title), ""); }
			set { propertyStorage.SetGlobalProperty(nameof(Title), value, ""); }
		}

		[DefaultValue("")]
		[Category(CategoryName)]
		public string Description
		{
			get { return propertyStorage.GetGlobalProperty(nameof(Description), ""); }
			set { propertyStorage.SetGlobalProperty(nameof(Description), value, ""); }
		}

		[DefaultValue("")]
		[Category(CategoryName)]
		public string Summary
		{
			get { return propertyStorage.GetGlobalProperty(nameof(Summary), ""); }
			set { propertyStorage.SetGlobalProperty(nameof(Summary), value, ""); }
		}

		[DefaultValue("")]
		[DisplayName("Language")]
		[Category(CategoryName)]
		public string NeutralLanguage
		{
			get { return propertyStorage.GetGlobalProperty(nameof(NeutralLanguage), ""); }
			set { propertyStorage.SetGlobalProperty(nameof(NeutralLanguage), value, ""); }
		}

		[DefaultValue("")]
		[Category(CategoryName)]
		public string Copyright
		{
			get { return propertyStorage.GetGlobalProperty(nameof(Copyright), ""); }
			set { propertyStorage.SetGlobalProperty(nameof(Copyright), value, ""); }
		}

		[DefaultValue(false)]
		[DisplayName("Require License Acceptance")]
		[Category(CategoryName)]
		public bool PackageRequireLicenseAcceptance
		{
			get { return propertyStorage.GetGlobalProperty(nameof(PackageRequireLicenseAcceptance), false); }
			set { propertyStorage.SetGlobalProperty(nameof(PackageRequireLicenseAcceptance), value, false); }
		}

		[DefaultValue("")]
		[DisplayName("License Url")]
		[Category(CategoryName)]
		public string PackageLicenseUrl
		{
			get { return propertyStorage.GetGlobalProperty(nameof(PackageLicenseUrl), ""); }
			set { propertyStorage.SetGlobalProperty(nameof(PackageLicenseUrl), value, ""); }
		}

		[DefaultValue("")]
		[DisplayName("Project Url")]
		[Category(CategoryName)]
		public string PackageProjectUrl
		{
			get { return propertyStorage.GetGlobalProperty(nameof(PackageProjectUrl), ""); }
			set { propertyStorage.SetGlobalProperty(nameof(PackageProjectUrl), value, ""); }
		}

		[DefaultValue("")]
		[DisplayName("Icon Url")]
		[Category(CategoryName)]
		public string PackageIconUrl
		{
			get { return propertyStorage.GetGlobalProperty(nameof(PackageIconUrl), ""); }
			set { propertyStorage.SetGlobalProperty(nameof(PackageIconUrl), value, ""); }
		}

		[DefaultValue("")]
		[Description("A space-delimited list of tags and keywords that describe the package.")]
		[DisplayName("Tags")]
		[Category(CategoryName)]
		public string PackageTags
		{
			get { return propertyStorage.GetGlobalProperty(nameof(PackageTags), ""); }
			set { propertyStorage.SetGlobalProperty(nameof(PackageTags), value, ""); }
		}

		[DefaultValue("")]
		[DisplayName("Release Notes")]
		[Category(CategoryName)]
		public string PackageReleaseNotes
		{
			get { return propertyStorage.GetGlobalProperty(nameof(PackageReleaseNotes), ""); }
			set { propertyStorage.SetGlobalProperty(nameof(PackageReleaseNotes), value, ""); }
		}

		[DefaultValue("")]
		[DisplayName("Repository Url")]
		[Category(CategoryName)]
		public string RepositoryUrl
		{
			get { return propertyStorage.GetGlobalProperty(nameof(RepositoryUrl), ""); }
			set { propertyStorage.SetGlobalProperty(nameof(RepositoryUrl), value, ""); }
		}

		[DefaultValue("")]
		[DisplayName("Repository Type")]
		[Category(CategoryName)]
		public string RepositoryType
		{
			get { return propertyStorage.GetGlobalProperty(nameof(RepositoryType), ""); }
			set { propertyStorage.SetGlobalProperty(nameof(RepositoryType), value, ""); }
		}

		[DefaultValue("")]
		[Description("A comma-delimited list of package types that indicate how a package is intended to be used, such as 'Dependency' and 'DotnetCliTool'.")]
		[DisplayName("Package Types")]
		[Category(CategoryName)]
		public string PackageTypes
		{
			get { return propertyStorage.GetGlobalProperty(nameof(PackageTypes), ""); }
			set { propertyStorage.SetGlobalProperty(nameof(PackageTypes), value, ""); }
		}
	}
}
