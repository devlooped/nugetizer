using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Packaging.VisualStudio.LegacyProjectSystem
{
	[PartCreationPolicy(CreationPolicy.Shared)]
	class NuSpecPropertyPageExports
	{
		const string MonoTouchProjectTypeGuid = "{6BC8ED88-2882-458C-8E55-DFD12B67127B}";
		const string MonoAndroidProjectTypeGuid = "{EFBA0AD7-5A72-4C68-AF49-83D382785DCF}";

		[Export(MonoTouchProjectTypeGuid)]
		public string NuSpecPropertyPageForIOS
		{
			get { return typeof(NuSpecPropertyPage).GUID.ToString("B"); }
		}

		[Export(MonoAndroidProjectTypeGuid)]
		public string NuSpecPropertyPageForAndroid
		{
			get { return typeof(NuSpecPropertyPage).GUID.ToString("B"); }
		}
	}
}
