using NuGet.PackageManagement.VisualStudio;
using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using NuGet.ProjectManagement;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Shell;
using NuGet.PackageManagement.UI;

namespace NuGet.Client.Fix
{
	[Export(typeof(IProjectSystemProvider))]
	[Name(nameof(LegacyCSProjProviderForNuProj))]
	[Order(Before = nameof(LegacyCSProjPackageReferenceProjectProvider))]
	class LegacyCSProjProviderForNuProj : IProjectSystemProvider
	{
		// Reason it's lazy<object> is because we don't want to load any CPS assemblies untill
		// we're really going to use any of CPS api. Which is why we also don't use nameof or typeof apis.
		[Import("Microsoft.VisualStudio.ProjectSystem.IProjectServiceAccessor")]
		private Lazy<object> ProjectServiceAccessor { get; set; }

		public bool TryCreateNuGetProject(Project dteProject, ProjectSystemProviderContext context, out NuGetProject result)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			result = null;

			var project = new EnvDTEProjectAdapter(dteProject);

			if (VsHierarchyUtility.ToVsHierarchy(dteProject).IsCapabilityMatch("PackagingProject"))
			{
				// Lazy load the CPS enabled JoinableTaskFactory for the UI.
				NuGetUIThreadHelper.SetJoinableTaskFactoryFromService(ProjectServiceAccessor.Value as Microsoft.VisualStudio.ProjectSystem.IProjectServiceAccessor);

				result = new LegacyCSProjPackageReferenceProject(
					project,
					VsHierarchyUtility.GetProjectId(dteProject));

				return true;
			}

			return false;
		}
	}
}