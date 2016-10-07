using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Packaging.VisualStudio
{
	public class AddPlatformImplementationViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public ObservableCollection<PlatformViewModel> Platforms { get; } =
			new ObservableCollection<PlatformViewModel>();

		public bool UseSharedProject { get; set; } = true;

		public bool IsSharedProjectEnabled { get; set; } = true;
	}
}
