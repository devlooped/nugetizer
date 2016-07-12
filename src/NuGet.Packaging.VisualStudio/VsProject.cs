using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NuGet.Packaging.VisualStudio
{
	class VsProject : IProject
	{
		readonly IVsHierarchy project;

		public VsProject(IVsHierarchy project)
		{
			this.project = project;
		}

		public bool IsNuProjProject => IsProjectSubtype(new Guid(Guids.FlavoredProjectTypeGuid)) ||
			DteProject.FullName.EndsWith(Constants.ProjectFileExtension, StringComparison.OrdinalIgnoreCase);

		Project DteProject
		{
			get
			{
				object dteProject;
				ErrorHandler.ThrowOnFailure(
					project.GetProperty(
						VSConstants.VSITEMID_ROOT,
						(int)__VSHPROPID.VSHPROPID_ExtObject,
						out dteProject));

				return dteProject as Project;
			}
		}

		bool IsProjectSubtype(Guid projectTypeGuid) =>
			GetProjectSubtypes().Any(type => type == projectTypeGuid);

		IEnumerable<Guid> GetProjectSubtypes()
		{
			string guidList;
			var aggregatableProject = project as IVsAggregatableProject;
			if (aggregatableProject != null &&
				ErrorHandler.Succeeded(aggregatableProject.GetAggregateProjectTypeGuids(out guidList)))
				return ParseGuids(guidList);

			return Enumerable.Empty<Guid>();
		}

		IEnumerable<Guid> ParseGuids(string guidList)
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

		public void BuildNuGetPackage()
		{
			if (!IsNuProjProject)
			{
				// TODO: Add NuProj type guid and targets
			}

			// TODO: Invoke the corresponding msbuild target
		}

		public void OpenNuSpecPropertyPage()
		{
			Guid projectDesignerEditorGuid;
			ErrorHandler.ThrowOnFailure(
				project.GetGuidProperty(
					VSConstants.VSITEMID_ROOT,
					(int)__VSHPROPID2.VSHPROPID_ProjectDesignerEditor,
					out projectDesignerEditorGuid));

			var vsProject = project as IVsProject2;

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