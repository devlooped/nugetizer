using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Packaging.VisualStudio
{
	class CrossPlatformViewModel
	{
		public CrossPlatformViewModel()
		{ }

		public CrossPlatformViewModel(IEnumerable<PlatformViewModel> platforms)
		{
			foreach (var platform in platforms)
				Platforms.Add(platform);
		}

		public ObservableCollection<PlatformViewModel> Platforms { get; } =
			new ObservableCollection<PlatformViewModel>();
	}
}
