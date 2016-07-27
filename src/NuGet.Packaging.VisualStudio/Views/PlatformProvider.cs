using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Packaging.VisualStudio
{
	[Export(typeof(IPlatformProvider))]
	class PlatformProvider : IPlatformProvider
	{
		public IEnumerable<PlatformViewModel> GetSupportedPlatforms()
		{
			yield return new PlatformViewModel(Constants.Platforms.IOS, Resources.IOS_DisplayName);
			yield return new PlatformViewModel(Constants.Platforms.Android, Resources.Android_DisplayName);
		}
	}
}