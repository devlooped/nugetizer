using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Packaging.VisualStudio
{
	interface IUnfoldPlatformTemplateService
	{
		void UnfoldTemplate(string platformId, string path, bool appendPlatformSuffix = true);
	}
}
