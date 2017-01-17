using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using VSLangProj;
using VSLangProj140;

namespace NuGet.Packaging.VisualStudio
{
	class AnalyzerReferencesService : AnalyzerReferences
	{
		Lazy<VSProject> project;

		public AnalyzerReferencesService(Lazy<VSProject> project)
		{
			this.project = project;
		}

		public void Add(string bstrPath)
		{
		}

		public void Remove(string bstrPath)
		{
		}

		public string Item(object index)
		{
			return null;
		}

		public IEnumerator GetEnumerator()
		{
			yield break;
		}

		public DTE DTE => project.Value.DTE;

		public dynamic Parent => project.Value;

		public Project ContainingProject => project.Value.Project;

		public int Count => 0;
	}
}
