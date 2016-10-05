using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace NuGet.Packaging.VisualStudio
{
	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	public class NuGetExtender 
	{
		const string CategoryName = "NuGet";
		const string BuildCategoryName = "NuGet Build";

		readonly IVsBuildPropertyStorage propertyStorage;

		internal NuGetExtender(IVsHierarchy hierarchy)
		{
			propertyStorage = (IVsBuildPropertyStorage)hierarchy;
		}

		[DefaultValue(true)]
		[Description("Whether to include symbols in the package if 'Include Outputs' is 'true'. Defaults to 'true' for debug builds, 'false' otherwise.")]
		[DisplayName("Include Symbols")]
		[Category(BuildCategoryName)]
		public bool IncludeSymbols
		{
			get { return GetGlobalProperty(nameof(IncludeSymbols), true); }
			set { SetGlobalProperty(nameof(IncludeSymbols), value, true); }
		}

		[DefaultValue(true)]
		[Description("Whether to include the primary outputs of this project in the package. Defaults to 'true'.")]
		[DisplayName("Include Outputs")]
		[Category(BuildCategoryName)]
		public bool IncludeOutputs
		{
			get { return GetGlobalProperty(nameof(IncludeOutputs), true); }
			set { SetGlobalProperty(nameof(IncludeOutputs), value, true); }
		}

		[DefaultValue("")]
		[Description("The output directory for the generated NuGet package. Defaults to the project's $(OutputPath) property.")]
		[DisplayName("Output Path")]
		[Category(BuildCategoryName)]
		public string PackageOutputPath
		{
			get { return GetGlobalProperty(nameof(PackageOutputPath), ""); }
			set { SetGlobalProperty(nameof(PackageOutputPath), value, ""); }
		}

		[DefaultValue(false)]
		[Description("Specifies if the output NuGet package should be generated on build. Only applies to projects that have a package Id set.")]
		[DisplayName("Pack On Build")]
		[Category(BuildCategoryName)]
		public bool PackOnBuild
		{
			get { return GetGlobalProperty(nameof(PackOnBuild), false); }
			set { SetGlobalProperty(nameof(PackOnBuild), value, false); }
		}

		[DefaultValue("")]
		[Description("Package identifier.")]
		[DisplayName("Id")]
		[Category(CategoryName)]
		public string PackageId
		{
			get { return GetGlobalProperty(nameof(PackageId), ""); }
			set { SetGlobalProperty(nameof(PackageId), value, ""); }
		}

		[DefaultValue("")]
		[Description("Package version.")]
		[DisplayName("Version")]
		[Category(CategoryName)]
		public string PackageVersion
		{
			get { return GetGlobalProperty(nameof(PackageVersion), ""); }
			set { SetGlobalProperty(nameof(PackageVersion), value, ""); }
		}

		[DefaultValue(false)]
		[Description("Specifies if the project generates a development dependency package.")]
		[DisplayName("Is Development Dependency")]
		[Category(CategoryName)]
		public bool IsDevelopmentDependency
		{
			get { return GetGlobalProperty(nameof(IsDevelopmentDependency), false); }
			set { SetGlobalProperty(nameof(IsDevelopmentDependency), value, false); }
		}

		[DefaultValue("")]
		[Category(CategoryName)]
		public string Authors
		{
			get { return GetGlobalProperty(nameof(Authors), ""); }
			set { SetGlobalProperty(nameof(Authors), value, ""); }
		}

		[DefaultValue("")]
		[Category(CategoryName)]
		public string Title
		{
			get { return GetGlobalProperty(nameof(Title), ""); }
			set { SetGlobalProperty(nameof(Title), value, ""); }
		}

		[DefaultValue("")]
		[Category(CategoryName)]
		public string Description
		{
			get { return GetGlobalProperty(nameof(Description), ""); }
			set { SetGlobalProperty(nameof(Description), value, ""); }
		}

		[DefaultValue("")]
		[Category(CategoryName)]
		public string Summary
		{
			get { return GetGlobalProperty(nameof(Summary), ""); }
			set { SetGlobalProperty(nameof(Summary), value, ""); }
		}

		[DefaultValue("")]
		[DisplayName("Language")]
		[Category(CategoryName)]
		public string NeutralLanguage
		{
			get { return GetGlobalProperty(nameof(NeutralLanguage), ""); }
			set { SetGlobalProperty(nameof(NeutralLanguage), value, ""); }
		}

		[DefaultValue("")]
		[Category(CategoryName)]
		public string Copyright
		{
			get { return GetGlobalProperty(nameof(Copyright), ""); }
			set { SetGlobalProperty(nameof(Copyright), value, ""); }
		}

		[DefaultValue(false)]
		[DisplayName("Require License Acceptance")]
		[Category(CategoryName)]
		public bool PackageRequireLicenseAcceptance
		{
			get { return GetGlobalProperty(nameof(PackageRequireLicenseAcceptance), false); }
			set { SetGlobalProperty(nameof(PackageRequireLicenseAcceptance), value, false); }
		}

		[DefaultValue("")]
		[DisplayName("License Url")]
		[Category(CategoryName)]
		public string PackageLicenseUrl
		{
			get { return GetGlobalProperty(nameof(PackageLicenseUrl), ""); }
			set { SetGlobalProperty(nameof(PackageLicenseUrl), value, ""); }
		}

		[DefaultValue("")]
		[DisplayName("Project Url")]
		[Category(CategoryName)]
		public string PackageProjectUrl
		{
			get { return GetGlobalProperty(nameof(PackageProjectUrl), ""); }
			set { SetGlobalProperty(nameof(PackageProjectUrl), value, ""); }
		}

		[DefaultValue("")]
		[DisplayName("Icon Url")]
		[Category(CategoryName)]
		public string PackageIconUrl
		{
			get { return GetGlobalProperty(nameof(PackageIconUrl), ""); }
			set { SetGlobalProperty(nameof(PackageIconUrl), value, ""); }
		}

		[DefaultValue("")]
		[Description("A space-delimited list of tags and keywords that describe the package.")]
		[DisplayName("Tags")]
		[Category(CategoryName)]
		public string PackageTags
		{
			get { return GetGlobalProperty(nameof(PackageTags), ""); }
			set { SetGlobalProperty(nameof(PackageTags), value, ""); }
		}

		[DefaultValue("")]
		[DisplayName("Release Notes")]
		[Category(CategoryName)]
		public string PackageReleaseNotes
		{
			get { return GetGlobalProperty(nameof(PackageReleaseNotes), ""); }
			set { SetGlobalProperty(nameof(PackageReleaseNotes), value, ""); }
		}

		[DefaultValue("")]
		[DisplayName("Repository Url")]
		[Category(CategoryName)]
		public string RepositoryUrl
		{
			get { return GetGlobalProperty(nameof(RepositoryUrl), ""); }
			set { SetGlobalProperty(nameof(RepositoryUrl), value, ""); }
		}

		[DefaultValue("")]
		[DisplayName("Repository Type")]
		[Category(CategoryName)]
		public string RepositoryType
		{
			get { return GetGlobalProperty(nameof(RepositoryType), ""); }
			set { SetGlobalProperty(nameof(RepositoryType), value, ""); }
		}

		[DefaultValue("")]
		[Description("A comma-delimited list of package types that indicate how a package is intended to be used, such as 'Dependency' and 'DotnetCliTool'.")]
		[DisplayName("Package Types")]
		[Category(CategoryName)]
		public string PackageTypes
		{
			get { return GetGlobalProperty(nameof(PackageTypes), ""); }
			set { SetGlobalProperty(nameof(PackageTypes), value, ""); }
		}

		bool GetGlobalProperty(string propertyName, bool defaultValue)
		{
			var value = GetGlobalProperty(propertyName, defaultValue.ToString(CultureInfo.InvariantCulture).ToLowerInvariant());
			bool result;
			if (!bool.TryParse(value, out result))
			{
				return defaultValue;
			}

			return result;
		}

		void SetGlobalProperty(string propertyName, bool value, bool defaultValue)
		{
			SetGlobalProperty(propertyName, 
				value.ToString(CultureInfo.InvariantCulture).ToLowerInvariant(),
				defaultValue.ToString(CultureInfo.InvariantCulture).ToLowerInvariant());
		}

		string GetGlobalProperty(string propertyName, string defaultValue)
		{
			string value;

			// Get the evaluated property value with null condition.
			// Why null and not String.Empty?
			// There is performance and (functionality implications) for String.Empty condition. 
			// String.Empty will force us to reevaluate the project with Config = "" and Platform = "", read the value, 
			// and reevaluate back. This ensures that only not conditional property value is read.But the price is we reevaluating twice. 
			// "null" means we will just read the value using the current evaluation setitings. 
			// If property is defined in "gobal" secitopn this is the same.
			// If property is "overriden" in "confug" section it will in fact get this value, 
			// but apart that the contract is already broken, that would be the value 
			// that will be assigned during build for task to access anyway so it will be the "truth".
			int hr = propertyStorage.GetPropertyValue(propertyName, null, (uint)_PersistStorageType.PST_PROJECT_FILE, out value);
			if (!ErrorHandler.Succeeded(hr) || string.IsNullOrEmpty(value))
			{
				return defaultValue;
			}

			return value;
		}

		void SetGlobalProperty(string propertyName, string value, string defaultValue)
		{
			if (string.IsNullOrEmpty(value) || value == defaultValue)
			{
				propertyStorage.RemoveProperty(
					propertyName,
					string.Empty,
					(uint)_PersistStorageType.PST_PROJECT_FILE);
			}
			else
			{
				// Set the property with String.Empty condition to ensure it's global (not configuration scoped).
				propertyStorage.SetPropertyValue(
					propertyName,
					string.Empty,
					(uint)_PersistStorageType.PST_PROJECT_FILE,
					value);
			}
		}
	}
}
