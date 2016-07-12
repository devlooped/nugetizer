using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using NuGet.Packaging.VisualStudio;
using System;

namespace EnvDTE
{
	static class DteProjectExtensions
	{
		internal static IProject AsProject(this Project project) =>
			new NuGet.Packaging.VisualStudio.VsProject(project.GetIVsHierarchy());

		static IVsHierarchy GetIVsHierarchy(this Project project)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var solution = Package.GetGlobalService(typeof(SVsSolution)) as IVsSolution;

			IVsHierarchy projectHierarchy;
			ErrorHandler.ThrowOnFailure(
				solution.GetProjectOfUniqueName(project.UniqueName, out projectHierarchy));

			return projectHierarchy;
		}
	}
}