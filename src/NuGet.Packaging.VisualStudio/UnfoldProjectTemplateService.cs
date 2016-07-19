using EnvDTE80;
using System.ComponentModel.Composition;
using System.IO;

namespace NuGet.Packaging.VisualStudio
{
	[Export(typeof(IUnfoldProjectTemplateService))]
	class UnfoldProjectTemplateService : IUnfoldProjectTemplateService
	{
		const string DefaultLanguage = "CSharp";

		readonly Solution2 solution;

		[ImportingConstructor]
		public UnfoldProjectTemplateService([Import(ExportProvider.DteSolution2)] object solution)
			: this((Solution2)solution)
		{ }

		public UnfoldProjectTemplateService(Solution2 solution)
		{
			this.solution = solution;
		}

		public bool IsTemplateInstalled(string templateId, string language = "")
		{
			try
			{
				if (string.IsNullOrEmpty(language))
					language = DefaultLanguage;

				return !string.IsNullOrEmpty(solution.GetProjectTemplate(templateId, language));
			}
			catch
			{
				return false;
			}
		}

		public void UnfoldTemplate(string templateId, string path, string language = "")
		{
			if (string.IsNullOrEmpty(language))
				language = DefaultLanguage;

			var projectTemplatePath = solution.GetProjectTemplate(templateId, language);

			if (!string.IsNullOrEmpty(projectTemplatePath))
			{
				solution.AddFromTemplate(
					projectTemplatePath,
					path,
					Path.GetFileName(path));
			}
		}
	}
}