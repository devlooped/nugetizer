using System;
using System.Collections.Generic;
using System.Linq;

namespace NuGet.Packaging.VisualStudio
{
	class PackageMetadataViewModel
	{
		IPropertyStorage storage;

		/// <summary>
		/// Default ctor for design data context
		/// </summary>
		public PackageMetadataViewModel()
			: this(new InMemoryPropertyStorage())
		{ }

		public PackageMetadataViewModel(IPropertyStorage storage)
		{
			this.storage = storage;
		}

		public string Authors
		{
			get { return storage.GetPropertyValue<string>(); }
			set { storage.SetPropertyValue(value); }
		}

		public string Copyright
		{
			get { return storage.GetPropertyValue<string>(); }
			set { storage.SetPropertyValue(value); }
		}

		public string PackageId
		{
			get { return storage.GetPropertyValue<string>(); }
			set { storage.SetPropertyValue(value); }
		}

		public string Owners
		{
			get { return storage.GetPropertyValue<string>(); }
			set { storage.SetPropertyValue(value); }
		}

		public string ProjectUrl
		{
			get { return storage.GetPropertyValue<string>(); }
			set { storage.SetPropertyValue(value); }
		}

		public string LicenseUrl
		{
			get { return storage.GetPropertyValue<string>(); }
			set { storage.SetPropertyValue(value); }
		}

		public string IconUrl
		{
			get { return storage.GetPropertyValue<string>(); }
			set { storage.SetPropertyValue(value); }
		}

		public string Summary
		{
			get { return storage.GetPropertyValue<string>(); }
			set { storage.SetPropertyValue(value); }
		}

		public string Tags
		{
			get { return storage.GetPropertyValue<string>(); }
			set { storage.SetPropertyValue(value); }
		}

		public string Title
		{
			get { return storage.GetPropertyValue<string>(); }
			set { storage.SetPropertyValue(value); }
		}

		public string PackageVersion
		{
			get { return storage.GetPropertyValue<string>(); }
			set { storage.SetPropertyValue(value); }
		}

		public string Description
		{
			get { return storage.GetPropertyValue<string>(); }
			set { storage.SetPropertyValue(value); }
		}

		public bool PackageRequireLicenseAcceptance
		{
			get { return storage.GetPropertyValue<bool>(); }
			set { storage.SetPropertyValue(value); }
		}

		public bool IsDevelopmentDependency
		{
			get { return storage.GetPropertyValue<bool>(); }
			set { storage.SetPropertyValue(value); }
		}

		public string PackageReleaseNotes
		{
			get { return storage.GetPropertyValue<string>(); }
			set { storage.SetPropertyValue(value); }
		}

		public string NeutralLanguage
		{
			get { return storage.GetPropertyValue<string>(); }
			set { storage.SetPropertyValue(value); }
		}
	}
}
