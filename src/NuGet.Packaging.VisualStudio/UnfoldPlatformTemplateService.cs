using Clide;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Packaging.VisualStudio
{
	[Export(typeof(IUnfoldPlatformTemplateService))]
	class UnfoldPlatformTemplateService : IUnfoldPlatformTemplateService
	{
		readonly IUnfoldTemplateService unfoldTemplateService;
		readonly Dictionary<string, PlatformTemplate> platformTemplates =
			new Dictionary<string, PlatformTemplate>();

		[ImportingConstructor]
		public UnfoldPlatformTemplateService(IUnfoldProjectTemplateService unfoldTemplateService)
		{
			this.unfoldTemplateService = unfoldTemplateService;

			PopulatePlatformTemplates();
		}

		void PopulatePlatformTemplates()
		{
			platformTemplates.Add(
				Constants.Platforms.IOS,
				new PlatformTemplate
				{
					TemplateId = Constants.Templates.IOS,
					Language = "CSharp",
				});

			platformTemplates.Add(
				Constants.Platforms.Android,
				new PlatformTemplate
				{
					TemplateId = Constants.Templates.Android,
					Language = "CSharp",
				});
		}

		public IProjectNode UnfoldTemplate(string platformId, string path)
		{
			PlatformTemplate template;
			if (!platformTemplates.TryGetValue(platformId, out template))
				throw new NotSupportedException(
					string.Format(
						Resources.UnfoldPlatformTemplateService_PlatformTemplateNotFound,
						platformId));

			return unfoldTemplateService.UnfoldTemplate(template.TemplateId, path, template.Language);
		}

		class PlatformTemplate
		{
			public string TemplateId { get; set; }
			public string Language { get; set; }
		}
	}
}