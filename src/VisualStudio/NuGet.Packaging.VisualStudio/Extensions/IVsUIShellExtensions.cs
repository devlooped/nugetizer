using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualStudio.Shell.Interop;
using System.Windows.Interop;
using Microsoft.VisualStudio;

namespace NuGet.Packaging.VisualStudio
{
	static class IVsUIShellExtensions
	{
		internal static void SetOwner(this IVsUIShell uiShell, Window dialog)
		{
			IntPtr owner;
			if (ErrorHandler.Succeeded(uiShell.GetDialogOwnerHwnd(out owner)))
			{
				new WindowInteropHelper(dialog).Owner = owner;
				dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
				dialog.ShowInTaskbar = false;
			}
		}
	}
}