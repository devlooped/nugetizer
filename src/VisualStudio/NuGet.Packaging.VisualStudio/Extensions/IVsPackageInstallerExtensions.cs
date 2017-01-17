using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.VisualStudio
{
	static class IVsPackageInstallerExtensions
	{
		public static void InstallBuildPackagingNuget(this IVsPackageInstaller packageInstaller, Project project) =>
			packageInstaller.InstallPackagesFromVSExtensionRepository(
				NuGet.Packaging.VisualStudio.Constants.NuGet.RepositoryId,
				false,
				true,
				project,
				new Dictionary<string, string>
				{
					{
						NuGet.Packaging.VisualStudio.Constants.NuGet.BuildPackagingId ,
						NuGet.Packaging.VisualStudio.Constants.NuGet.BuildPackagingVersion }
				});
	}
}
