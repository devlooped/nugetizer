using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NuGet.Packaging.VisualStudio
{
	[Export(typeof(IDialogService))]
	class DialogService : IDialogService
	{
		readonly IVsUIShell uiShell;

		[ImportingConstructor]
		public DialogService([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
		{
			this.uiShell = serviceProvider.GetService<SVsUIShell, IVsUIShell>();
		}

		public bool ShowConfirmationMessage(string message) =>
			System.Windows.Forms.MessageBox.Show(
				message,
				Resources.DefaultDialogCaption,
				System.Windows.Forms.MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK;

		public bool? ShowDialog<T>(T dialog) where T : Window
		{
			uiShell.SetOwner(dialog);

			return dialog.ShowDialog();
		}
	}
}
