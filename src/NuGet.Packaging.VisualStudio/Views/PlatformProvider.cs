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
			yield return new PlatformViewModel
			{
				DisplayName = Resources.IOS_DisplayName,
				Id = Constants.Platforms.IOS
			};

			yield return new PlatformViewModel
			{
				DisplayName = Resources.Android_DisplayName,
				Id = Constants.Platforms.Android
			};
		}
	}
}
