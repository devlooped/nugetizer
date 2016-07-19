using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Packaging.VisualStudio
{
	interface IProject
	{
		string Name { get; }

		string Path { get; }

		bool IsNuProj { get; }

		void BuildNuGetPackage();

		void OpenNuSpecPropertyPage();
	}
}
