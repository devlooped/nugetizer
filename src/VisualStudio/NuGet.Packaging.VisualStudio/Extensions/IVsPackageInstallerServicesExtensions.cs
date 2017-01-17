using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.VisualStudio
{
	static class IVsPackageInstallerServicesExtensions
	{
		public static bool IsBuildPackagingNuGetInstalled(this IVsPackageInstallerServices packageInstallerServices, Project project) =>
			packageInstallerServices.IsPackageInstalled(project, NuGet.Packaging.VisualStudio.Constants.NuGet.BuildPackagingId);
	}
}
