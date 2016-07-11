using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace EnvDTE
{
	static class DteProjectExtensions
	{
		internal static void OpenPropertyPage(this Project project, string propertyPageGuid)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var projectHierarchy = project.GetIVsHierarchy();

			Guid projectDesignerEditorGuid;
			ErrorHandler.ThrowOnFailure(
				projectHierarchy.GetGuidProperty(
					(uint)VSConstants.VSITEMID.Root,
					(int)__VSHPROPID2.VSHPROPID_ProjectDesignerEditor,
					out projectDesignerEditorGuid));

			var vsProject = projectHierarchy as IVsProject2;

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
				multiViewDocView.ActivateLogicalView(new Guid(propertyPageGuid)));
		}

		internal static IVsHierarchy GetIVsHierarchy(this Project project)
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