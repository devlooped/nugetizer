using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Packaging.VisualStudio
{
	public class MultiPlatformViewModel
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public ObservableCollection<PlatformViewModel> Platforms { get; } =
			new ObservableCollection<PlatformViewModel>();

		public bool UsePlatformSpecific { get; set; } = true;

		public bool UseSinglePlatform { get; set; } = false;
	}
}
