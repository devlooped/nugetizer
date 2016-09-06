using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NuGet.Packaging.VisualStudio
{
	interface IDialogService
	{
		bool ShowConfirmationMessage(string message);
		bool? ShowDialog<T>(T dialog) where T : Window;
	}
}
