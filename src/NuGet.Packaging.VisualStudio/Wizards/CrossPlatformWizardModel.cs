using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Packaging.VisualStudio
{
	class CrossPlatformWizardModel
	{
		public virtual void ParseParameters(Dictionary<string, string> replacementsDictionary)
		{
			foreach (var param in replacementsDictionary)
			{
				switch (param.Key)
				{
					case "$safeprojectname$": SafeProjectName = param.Value; break;
					case "$solutiondirectory$": SolutionDirectory = param.Value; break;
				}
			}
		}

		public string SafeProjectName { get; internal set; }

		public string SolutionDirectory { get; internal set; }
	}
}
