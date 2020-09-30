using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using static ThisAssembly.Strings;
using NuGet.Packaging;

namespace NuGetizer.Tasks
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
        public string IsPackaging { get; set; }

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
                StringComparer.OrdinalIgnoreCase);

            AssignedFiles = Files.Select(file => EnsurePackagePath(file, kindMap)).ToArray();

            return !Log.HasLoggedErrors;
        }

        ITaskItem EnsurePackagePath(ITaskItem file, IDictionary<string, ITaskItem> kindMap)
        {
            var output = new TaskItem(file);

            // Map the Kind to a target top-level directory.
            var kind = file.GetMetadata("Kind");
            var packageFolder = "";
            var frameworkSpecific = false;
            if (!string.IsNullOrEmpty(kind) && kindMap.TryGetValue(kind, out var kindItem))
            {
                packageFolder = kindItem.GetMetadata(MetadataName.PackageFolder);
                bool.TryParse(kindItem.GetMetadata(MetadataName.FrameworkSpecific), out frameworkSpecific);
            }
            else if (!string.IsNullOrEmpty(kind))
            {
                // By convention, we just turn the first letter of Kind to lowercase and assume that 
                // to be a valid folder kind.
                packageFolder = char.IsLower(kind[0]) ? kind :
                    char.ToLower(kind[0]).ToString() + kind.Substring(1);
            }

            // Specific PackageFile can always override Kind-inferred FrameworkSpecific value.
            if (bool.TryParse(file.GetMetadata(MetadataName.FrameworkSpecific), out var frameworkSpecificOverride))
                frameworkSpecific = frameworkSpecificOverride;

            output.SetMetadata(MetadataName.PackageFolder, packageFolder);

            // NOTE: a declared TargetFramework metadata trumps TargetFrameworkMoniker, 
            // which is defaulted to that of the project being built.
            var targetFramework = output.GetMetadata(MetadataName.TargetFramework);
            if (string.IsNullOrEmpty(targetFramework) && frameworkSpecific)
            {
                var frameworkMoniker = file.GetTargetFrameworkMoniker();
                targetFramework = frameworkMoniker.GetShortFrameworkName() ?? "";
                // At this point we have the correct target framework
                output.SetMetadata(MetadataName.TargetFramework, targetFramework);
            }

            // If PackagePath already specified, we're done.
            if (!string.IsNullOrEmpty(file.GetMetadata("PackagePath")))
                return output;

            // If a packaging project is requesting the package path assignment, 
            // perform it regardless of whether there is a PackageId on the items, 
            // since they all need to be assigned at this point.
            bool isPackaging = false;
            isPackaging = !string.IsNullOrEmpty(IsPackaging) &&
                bool.TryParse(IsPackaging, out isPackaging) &&
                isPackaging;

            // If no PackageId specified, we let referencing projects define the package path
            // as it will be included in their package.
            // NOTE: if we don't do this, the package path of a project would be determined 
            // by the declaring project's TFM, rather than the referencing one, and this 
            // would be incorrect (i.e. a referenced PCL project that does not build a 
            // nuget itself, would end up in the PCL lib folder rather than the referencing 
            // package's lib folder for its own TFM, i.e. 'lib\net45').
            if (string.IsNullOrEmpty(file.GetMetadata("PackageId")) && !isPackaging)
                return output;

            // If we got this far but there wasn't a Kind to process, it's an error.
            if (string.IsNullOrEmpty(kind))
            {
                Log.LogErrorCode(nameof(ErrorCode.NG0010), ErrorCode.NG0010(file.ItemSpec));
                // We return the file anyway, since the task result will still be false.
                return file;
            }

            // If the kind is known but it isn't mapped to a folder inside the package, we're done.
            // Special-case None kind since that means 'leave it wherever it lands' ;)
            if (string.IsNullOrEmpty(packageFolder) && kind != PackageItemKind.None)
                return output;

            // Special case for contentFiles, since they can also provide a codeLanguage metadata
            if (packageFolder == PackagingConstants.Folders.ContentFiles)
            {
                if (file.GetMetadata("TargetPath").StartsWith(packageFolder, StringComparison.OrdinalIgnoreCase))
                {
                    Log.LogErrorCode(nameof(ErrorCode.NG0013), ErrorCode.NG0013(file.GetMetadata("TargetPath")));
                    // We return the file anyway, since the task result will still be false.
                    return file;
                }

                // See https://docs.nuget.org/create/nuspec-reference#contentfiles-with-visual-studio-2015-update-1-and-later
                var codeLanguage = file.GetMetadata(MetadataName.ContentFile.CodeLanguage);
                if (string.IsNullOrEmpty(codeLanguage))
                {
                    codeLanguage = PackagingConstants.AnyCodeLanguage;
                    output.SetMetadata(MetadataName.ContentFile.CodeLanguage, codeLanguage);
                }
                else
                {
                    // This allows setting the codeLanguage to just the %(Extension) of the source file
                    codeLanguage = codeLanguage.TrimStart('.');
                }

                packageFolder = Path.Combine(packageFolder, codeLanguage);

                // And they also cannot have an empty framework, at most, it will be "any"
                if (string.IsNullOrEmpty(targetFramework))
                    targetFramework = PackagingConstants.AnyFramework;

                // Once TF is defaulted, a content file is actually always framework-specific, 
                // although the framework may be 'any'.
                frameworkSpecific = true;

                // At this point we have the correct target framework for a content file, so persist it.
                output.SetMetadata(MetadataName.TargetFramework, targetFramework);
            }

            var targetPath = file.GetMetadata("TargetPath");
            // Linked files already have the desired target path specified by the user
            if (string.IsNullOrEmpty(targetPath))
                targetPath = file.GetMetadata("Link");

            // NOTE: TargetPath allows a framework-specific file to still specify its relative 
            // location without hardcoding the target framework (useful for multi-targetting and 
            // P2P references).
            if (string.IsNullOrEmpty(targetPath))
            {
                targetPath = string.IsNullOrEmpty(packageFolder) ?
                    Path.Combine(file.GetMetadata("RelativeDir"), file.GetMetadata("FileName") + file.GetMetadata("Extension")) :
                    // Well-known folders only get root-level files by default. Can be overriden with PackagePath or TargetPath 
                    // explicitly, of course
                    file.GetMetadata("FileName") + file.GetMetadata("Extension");
            }

            if (!string.IsNullOrEmpty(packageFolder) &&
                targetPath.StartsWith(packageFolder + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                // Avoid duplicating already determined package folder in package path later on.
                targetPath = targetPath.Substring(packageFolder.Length + 1);
            }

            // At this point we have the correct calculated target path, so persist it for inspection if necessary.
            output.SetMetadata("TargetPath", targetPath);

            // If we have no known package folder, files go to their RelativeDir location.
            // This allows custom packaging paths such as "workbooks", "docs" or whatever, which aren't prohibited by 
            // the format.
            var packagePath = string.IsNullOrEmpty(packageFolder) ?
                // File goes to the determined target path (or the root of the package), such as a readme.txt
                targetPath :
                frameworkSpecific ?
                    // Otherwise, it goes to a framework-specific folder.
                    Path.Combine(new[] { packageFolder, targetFramework }.Concat(targetPath.Split(Path.DirectorySeparatorChar)).ToArray()) :
                    // Except if frameworkSpecific is false, such as for build, tools, runtimes
                    Path.Combine(new[] { packageFolder }.Concat(targetPath.Split(Path.DirectorySeparatorChar)).ToArray());

            output.SetMetadata(MetadataName.PackagePath, packagePath);

            return output;
        }
    }
}
