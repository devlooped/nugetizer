using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Packaging.VisualStudio
{
	interface IUnfoldTemplateService
	{
		bool IsTemplateInstalled(string templateId);
		void UnfoldTemplate(string templateId, string path);
	}

	interface IUnfoldProjectTemplateService : IUnfoldTemplateService { }

	interface IUnfoldProjectItemTemplateService : IUnfoldTemplateService { }
}
