using System;
using System.IO;
using System.Reflection;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace NuGet.Build.Packaging.Tasks
{
	public class GenerateAssemblyInfo : Task
	{
		[Required]
		public ITaskItem[] Assemblies { get; set; }

		[Required]
		public ITaskItem[] OutputDirectories { get; set; }

		[Output]
		public string AssemblyName { get; set; }

		public override bool Execute()
		{
			AssemblyName name = GetAssemblyName();
			CreateAssemblyInfo(name);

			AssemblyName = name.Name;

			return true;
		}

		AssemblyName GetAssemblyName()
		{
			AssemblyName[] assemblyNames = Assemblies.NullAsEmpty()
				.Select(assembly => System.Reflection.AssemblyName.GetAssemblyName(assembly.ItemSpec))
				.ToArray();

			string[] allNames = assemblyNames.Select(name => name.Name).ToArray();
			if (allNames.Distinct().Count() > 1)
			{
				Log.LogWarningCode("NG1003",
					BuildEngine.ProjectFileOfTaskNode,
					"Assembly names should be the same for a bait and switch NuGet package. Names: '{0}' Assemblies: {1}",
					string.Join(", ", allNames),
					string.Join(", ", Assemblies.Select(assembly => assembly.ItemSpec)));
			}

			Version[] allVersions = assemblyNames.Select(name => name.Version).ToArray();
			if (allVersions.Distinct().Count() > 1)
			{
				Log.LogWarningCode("NG1004",
					BuildEngine.ProjectFileOfTaskNode,
					"Assembly versions should be the same for a bait and switch NuGet package. Versions: '{0}' Assemblies: {1}",
					string.Join(", ", allVersions.Select(version => version.ToString())),
					string.Join(", ", Assemblies.Select(assembly => assembly.ItemSpec)));
			}

			return assemblyNames.FirstOrDefault();
		}

		void CreateAssemblyInfo(AssemblyName assemblyName)
		{
			string text = "using System.Reflection;" + Environment.NewLine +
				string.Format("[assembly: AssemblyVersion (\"{0}\")]", assemblyName.Version);

			foreach (ITaskItem directory in OutputDirectories.NullAsEmpty())
			{
				string fileName = Path.Combine(directory.ItemSpec, "AssemblyInfo.cs");
				File.WriteAllText(fileName, text);
			}
		}
	}
}