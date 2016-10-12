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
