using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Packaging.VisualStudio
{
	class Constants
	{
		/// <summary>
		/// The file extension of this project type.  No preceding period.
		/// </summary>
		public const string ProjectExtension = "nuproj";
		public const string ProjectFileExtension = "." + ProjectExtension;

		public const string Language = "NuGet.Packaging";
		public const string NuGetPackageProjectTemplateId = "NuGet.Packaging.VisualStudio.Package";

	}
}
