using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace NuGet.Packaging.VisualStudio
{
	class PackageMetadataViewModel : ViewModelBase
	{
		readonly IPropertyStorage storage;

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

		[Required]
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

		[Required]
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

		public string PackageProjectUrl
		{
			get { return storage.GetPropertyValue<string>(); }
			set { storage.SetPropertyValue(value); }
		}

		public string PackageLicenseUrl
		{
			get { return storage.GetPropertyValue<string>(); }
			set { storage.SetPropertyValue(value); }
		}

		public string PackageIconUrl
		{
			get { return storage.GetPropertyValue<string>(); }
			set { storage.SetPropertyValue(value); }
		}

		public string Summary
		{
			get { return storage.GetPropertyValue<string>(); }
			set { storage.SetPropertyValue(value); }
		}

		public string PackageTags
		{
			get { return storage.GetPropertyValue<string>(); }
			set { storage.SetPropertyValue(value); }
		}

		public string Title
		{
			get { return storage.GetPropertyValue<string>(); }
			set { storage.SetPropertyValue(value); }
		}

		[Required]
		public string PackageVersion
		{
			get { return storage.GetPropertyValue<string>(); }
			set { storage.SetPropertyValue(value); }
		}

		[Required]
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
