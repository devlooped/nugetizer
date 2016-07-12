using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Packaging.VisualStudio
{
	interface IProject
	{
		bool IsNuProjProject { get; }

		void BuildNuGetPackage();

		void OpenNuSpecPropertyPage();
	}
}
