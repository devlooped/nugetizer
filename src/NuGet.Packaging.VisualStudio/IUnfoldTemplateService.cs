using Clide;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Packaging.VisualStudio
{
	// TODO: Move to Clide
	interface IUnfoldTemplateService
	{
		bool IsTemplateInstalled(string templateId, string language = "");
		IProjectNode UnfoldTemplate(string templateId, string path, string language = "");
	}

	interface IUnfoldProjectTemplateService : IUnfoldTemplateService { }
}
