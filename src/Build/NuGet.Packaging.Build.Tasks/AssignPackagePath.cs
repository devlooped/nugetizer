using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.NuGet.Packaging.Build.Tasks.Properties;
using NuGet.Packaging;
using static Microsoft.NuGet.Packaging.Build.Tasks.Properties.Strings;
using static NuGet.Client.ManagedCodeConventions;

namespace Microsoft.NuGet.Packaging.Build.Tasks
{
	/// <summary>
	/// Ensures all files have the PackagePath metadata.
	/// If the PackagePath was not explicitly specified, 
	/// determine one from the project relative	path and 
	/// the TargetFramework and Kind metadata, and set it 
	/// on the projected item.
	/// </summary>
	public class AssignPackagePath : Task
	{
		[Required]
		public ITaskItem[] Files { get; set; }

		[Required]
		public ITaskItem[] Kinds { get; set; }

		[Output]
		public ITaskItem[] AssignedFiles { get; set; }

		public override bool Execute()
		{
			var kindMap = Kinds.ToDictionary(
				kind => kind.ItemSpec, 
				kind => kind.GetMetadata("PackageFolder"), 
				StringComparer.OrdinalIgnoreCase);

			AssignedFiles = Files.Select(file => EnsurePackagePath(file, kindMap)).ToArray();

			return !Log.HasLoggedErrors;
		}

		ITaskItem EnsurePackagePath(ITaskItem file, IDictionary<string, string> kindMap)
		{
			if (!string.IsNullOrEmpty(file.GetMetadata("PackagePath")))
				return new TaskItem(file);

			var kind = file.GetMetadata("Kind");
			if (string.IsNullOrEmpty(kind))
			{
				Log.LogErrorCode(nameof(ErrorCode.NP0010), ErrorCode.NP0010(file.ItemSpec));
				// We return the file anyway, since the task result will still be false.
				return file;
			}

			// Map the Kind to a target top-level directory.
			string packageFolder;
			if (!kindMap.TryGetValue(kind, out packageFolder))
			{
				// By convention, we just turn the first letter of Kind to lowercase and assume that 
				// to be a valid folder kind.
				packageFolder = char.IsLower(kind[0]) ? kind :
					char.ToLower(kind[0]).ToString() + kind.Substring(1);
			}

			var output = new TaskItem(file);
			output.SetMetadata(MetadataName.PackageFolder, packageFolder);

			// Special case for contentFiles, since they can also provide a codeLanguage metadata
			if (packageFolder == PackagingConstants.Folders.ContentFiles)
			{
				/// See https://docs.nuget.org/create/nuspec-reference#contentfiles-with-visual-studio-2015-update-1-and-later
				var codeLanguage = file.GetMetadata(nameof(PropertyNames.CodeLanguage)) ?? PackagingConstants.AnyFramework;
				packageFolder = Path.Combine(packageFolder, codeLanguage);
			}

			var frameworkMoniker = file.GetTargetFrameworkMoniker();
			var targetFramework = frameworkMoniker.GetShortFrameworkName() ?? "";
			var fileName = file.GetMetadata("TargetPath");
			if (string.IsNullOrEmpty(fileName))
				fileName = file.GetMetadata("FileName") + file.GetMetadata("Extension");

			// None files go straight to the root folder of the package.
			var packagePath = string.IsNullOrEmpty(packageFolder) ?
				// File goes to the determined target path (or the root of the package), such as a readme.txt
				fileName :
				// Otherwise, it goes to a framework-specific folder.
				Path.Combine(new[] { packageFolder, targetFramework }.Concat(fileName.Split(Path.DirectorySeparatorChar)).ToArray());

			output.SetMetadata(MetadataName.TargetFramework, targetFramework);
			output.SetMetadata(MetadataName.PackagePath, packagePath);

			return output;
		}
	}
}
