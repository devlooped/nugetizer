using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Packaging.VisualStudio
{
	[Export(typeof(IDialogService))]
	class DialogService : IDialogService
	{
		public bool ShowConfirmationMessage(string message) =>
			System.Windows.Forms.MessageBox.Show(
				message,
				Resources.DefaultDialogCaption,
				System.Windows.Forms.MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK;
	}
}
