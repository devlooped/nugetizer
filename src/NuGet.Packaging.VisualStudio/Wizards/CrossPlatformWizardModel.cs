using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Packaging.VisualStudio
{
	class CrossPlatformWizardModel
	{
		protected List<PlatformTemplate> platformTemplates = new List<PlatformTemplate>();

		public virtual void ParseParameters(Dictionary<string, string> replacementsDictionary)
		{
			foreach (var param in replacementsDictionary)
			{
				PlatformTemplate platformTemplate;
				if (PlatformTemplate.TryParse(param.Key, param.Value, out platformTemplate))
				{
					platformTemplates.Add(platformTemplate);
				}
				else
				{
					switch (param.Key)
					{
						case "$safeprojectname$": SafeProjectName = param.Value; break;
						case "$solutiondirectory$": SolutionDirectory = param.Value; break;
					}
				}
			}
		}

		public IEnumerable<PlatformTemplate> PlatformTemplates => platformTemplates;

		public string SafeProjectName { get; internal set; }

		public string SolutionDirectory { get; internal set; }
	}
}
