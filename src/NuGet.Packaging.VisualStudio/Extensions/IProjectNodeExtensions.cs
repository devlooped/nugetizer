using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using NuGet.Packaging.VisualStudio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Clide
{
	static class IProjectNodeExtensions
	{
		public static bool IsNuProj(this IProjectNode project) => project.IsProjectSubtype(
				new Guid(Guids.FlavoredProjectTypeGuid)) ||
				project.As<EnvDTE.Project>().FullName.EndsWith(
					NuGet.Packaging.VisualStudio.Constants.ProjectFileExtension, StringComparison.OrdinalIgnoreCase);

		static bool IsProjectSubtype(this IProjectNode project, Guid projectTypeGuid) =>
			project.GetProjectSubtypes().Any(type => type == projectTypeGuid);

		static IEnumerable<Guid> GetProjectSubtypes(this IProjectNode project)
		{
			string guidList;
			var aggregatableProject = (IVsAggregatableProject)project.AsVsProject();
			if (aggregatableProject != null &&
				ErrorHandler.Succeeded(aggregatableProject.GetAggregateProjectTypeGuids(out guidList)))
				return ParseGuids(guidList);

			return Enumerable.Empty<Guid>();
		}

		static IEnumerable<Guid> ParseGuids(string guidList)
		{
			if (!string.IsNullOrEmpty(guidList))
			{
				foreach (var value in guidList.Trim().Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
				{
					Guid guid;
					if (Guid.TryParse(value.Trim(), out guid))
						yield return guid;
				}
			}
		}

		public static void BuildNuGetPackage(this IProjectNode project)
		{
		}

		public static void OpenNuSpecPropertyPage(this IProjectNode project)
		{
			var hierarchy = project.AsVsHierarchy();

			Guid projectDesignerEditorGuid;
			ErrorHandler.ThrowOnFailure(
				hierarchy.GetGuidProperty(
					VSConstants.VSITEMID_ROOT,
					(int)__VSHPROPID2.VSHPROPID_ProjectDesignerEditor,
					out projectDesignerEditorGuid));

			var vsProject = hierarchy as IVsProject2;

			IVsWindowFrame frame;
			ErrorHandler.ThrowOnFailure(
				vsProject.ReopenItem(
					VSConstants.VSITEMID_ROOT,
					ref projectDesignerEditorGuid,
					null,
					Guid.Empty,
					new IntPtr(-1),
					out frame));

			object docView;
			ErrorHandler.ThrowOnFailure(
				frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out docView));

			var multiViewDocView = docView as IVsMultiViewDocumentView;
			ErrorHandler.ThrowOnFailure(
				multiViewDocView.ActivateLogicalView(new Guid(Guids.NuSpecPropertyPageGuid)));
		}
	}
}
