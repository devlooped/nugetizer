using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Packaging.VisualStudio
{
	class PackageMetadataViewModel
	{
		public string Authors { get; set; }
		public string Copyright { get; set; }
		public string Id { get; set; }
		public string Owners { get; set; }
		public string ProjectUrl { get; set; }
		public string LicenseUrl { get; set; }
		public string IconUrl { get; set; }
		public string Summary { get; set; }
		public string Tags { get; set; }
		public string Title { get; set; }
		public string Version { get; set; }

		public string Description { get; set; }

		public bool RequireLicenseAcceptance { get; set; }
		public bool DevelopmentDependency { get; set; }
		public string ReleaseNotes { get; set; }
		public string Language { get; set; }
	}
}
