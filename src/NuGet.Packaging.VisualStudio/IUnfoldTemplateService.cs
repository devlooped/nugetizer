using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Packaging.VisualStudio
{
	interface IUnfoldTemplateService
	{
		bool IsTemplateInstalled(string templateId, string language = "");
		void UnfoldTemplate(string templateId, string path, string language = "");
	}

	interface IUnfoldProjectTemplateService : IUnfoldTemplateService { }

	interface IUnfoldProjectItemTemplateService : IUnfoldTemplateService { }
}
