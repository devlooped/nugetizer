using Clide;
using EnvDTE80;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

namespace NuGet.Packaging.VisualStudio
{
	[Export(typeof(IUnfoldProjectTemplateService))]
	class UnfoldProjectTemplateService : IUnfoldProjectTemplateService
	{
		const string DefaultLanguage = "CSharp";

		readonly ISolutionExplorer solutionExplorer;
		readonly Solution2 solution;

		[ImportingConstructor]
		public UnfoldProjectTemplateService(
			ISolutionExplorer solutionExplorer,
			[Import(ExportProvider.DteSolution2)] object solution)
			: this(solutionExplorer, (Solution2)solution)
		{ }

		public UnfoldProjectTemplateService(ISolutionExplorer solutionExplorer, Solution2 solution)
		{
			this.solutionExplorer = solutionExplorer;
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

		public IProjectNode UnfoldTemplate(string templateId, string path, string language = "")
		{
			if (string.IsNullOrEmpty(language))
				language = DefaultLanguage;

			var projectTemplatePath = solution.GetProjectTemplate(templateId, language);
			if (projectTemplatePath == null)
				throw new NotSupportedException(
					string.Format(
						Resources.UnfoldProjectTemplateService_TemplateNotFound,
						templateId,
						language));

			var projectName = Path.GetFileName(path);

			solution.AddFromTemplate(
				projectTemplatePath,
				path,
				projectName);

			var project = solutionExplorer
				.Solution
				.Nodes
				.OfType<IProjectNode>()
				.FirstOrDefault(x =>
					x.Name == projectName &&
					string.Equals(Path.GetDirectoryName(x.PhysicalPath), path, StringComparison.OrdinalIgnoreCase));

			if (project == null)
				throw new InvalidOperationException(
					string.Format(
						Resources.UnfoldProjectTemplateService_UnfoldTemplateFailed,
						templateId,
						language));

			return project;
		}
	}
}