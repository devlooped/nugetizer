using Clide;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Packaging.VisualStudio
{
	public class PlatformViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public PlatformViewModel()
		{ }

		public PlatformViewModel(string id, string displayName)
		{
			this.Id = id;
			this.DisplayName = displayName;
		}

		public bool IsSelected { get; set; } = true;

		public bool IsEnabled { get; set; } = true;

		public string DisplayName { get; set; }

		public string Id { get; set; }
	}
}
