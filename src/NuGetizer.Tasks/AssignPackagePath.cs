using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NuGet.Packaging;
using static ThisAssembly.Strings;

namespace NuGetizer.Tasks
{
    /// <summary>
    /// Ensures all files have the PackagePath metadata.
    /// If the PackagePath was not explicitly specified, 
    /// determine one from the project relative	path and 
    /// the TargetFramework and PackFolder metadata, and set it 
    /// on the projected item.
    /// </summary>
    public class AssignPackagePath : Task
    {
        public string IsPackaging { get; set; }

        [Required]
        public ITaskItem[] Files { get; set; }

        [Required]
        public ITaskItem[] KnownFolders { get; set; }

        [Output]
        public ITaskItem[] AssignedFiles { get; set; }

        public override bool Execute()
        {
            if (Environment.GetEnvironmentVariable("DEBUG_NUGETIZER") == "1")
                Debugger.Launch();

            var kindMap = KnownFolders.ToDictionary(
                kind => kind.ItemSpec,
                StringComparer.OrdinalIgnoreCase);

            AssignedFiles = Files.Select(file => EnsurePackagePath(file, kindMap)).ToArray();

            return !Log.HasLoggedErrors;
        }

        ITaskItem EnsurePackagePath(ITaskItem file, IDictionary<string, ITaskItem> kindMap)
        {
            // Switch to full path items
            var output = new TaskItem(
                file.GetBoolean("IsFile", true) && file.ItemSpec.IndexOfAny(Path.GetInvalidPathChars()) == -1 && File.Exists(file.GetMetadata("FullPath"))
                ? file.GetMetadata("FullPath")
                : file.ItemSpec);

            // Preserve existing metadata.
            file.CopyMetadataTo(output);

            // Map the pack folder to a target top-level directory.
            var packFolder = file.GetMetadata("PackFolder");
            var packageFolder = "";
            var frameworkSpecific = false;
            if (!string.IsNullOrEmpty(packFolder) && kindMap.TryGetValue(packFolder, out var kindItem))
            {
                packageFolder = kindItem.GetMetadata(MetadataName.PackageFolder);
                bool.TryParse(kindItem.GetMetadata(MetadataName.FrameworkSpecific), out frameworkSpecific);
            }
            else if (!string.IsNullOrEmpty(packFolder))
            {
                // By convention, we just turn the first letter of PackFolder to lowercase and assume that 
                // to be a valid folder kind.
                packageFolder = char.IsLower(packFolder[0]) ? packFolder :
                    char.ToLower(packFolder[0]).ToString() + packFolder.Substring(1);
            }

            // Specific PackageFile can always override PackFolder-inferred FrameworkSpecific value.
            if (bool.TryParse(file.GetMetadata(MetadataName.FrameworkSpecific), out var frameworkSpecificOverride))
                frameworkSpecific = frameworkSpecificOverride;

            output.SetMetadata(MetadataName.PackageFolder, packageFolder.Replace('\\', '/'));

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
            if (file.TryGetMetadata("PackagePath", out var packagePath))
            {
                // If PackagePath ends in directory separator, we assume 
                // the file/path needs to be appended too.
                if (packagePath.EndsWith("\\") || packagePath.EndsWith("/"))
                {
                    if (file.TryGetMetadata("Link", out var link))
                        packagePath = Path.Combine(packagePath, link);
                    else if (Path.IsPathRooted(file.GetMetadata("RelativeDir")))
                        // If the relative dir is absolute, we can just append the filename+extension instead
                        packagePath = Path.Combine(packagePath, file.GetMetadata("FileName") + file.GetMetadata("Extension"));
                    else
                        packagePath = Path.Combine(packagePath,
                            file.GetMetadata("RelativeDir"),
                            file.GetMetadata("FileName") +
                            file.GetMetadata("Extension"));
                }

                // Always normalize our output
                output.SetMetadata("PackagePath", packagePath.Replace('\\', '/'));
                return output;
            }

            // If a packaging project is requesting the package path assignment, 
            // perform it regardless of whether there is a PackageId on the items, 
            // since they all need to be assigned at this point.
            var isPackaging = false;
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

            // If we got this far but there wasn't a PackFolder to process, it's an error.
            if (string.IsNullOrEmpty(packFolder))
            {
                Log.LogErrorCode(nameof(ErrorCode.NG0010), ErrorCode.NG0010(file.ItemSpec));
                // We return the file anyway, since the task result will still be false.
                return file;
            }

            // If the kind is known but it isn't mapped to a folder inside the package, we're done.
            // Special-case None kind since that means 'leave it wherever it lands' ;)
            if (string.IsNullOrEmpty(packageFolder) && !packFolder.Equals(PackFolderKind.None, StringComparison.OrdinalIgnoreCase))
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
            output.SetMetadata("TargetPath", targetPath.Replace('\\', '/'));

            // If we have no known package folder, files go to their RelativeDir location.
            // This allows custom packaging paths such as "workbooks", "docs" or whatever, which aren't prohibited by 
            // the format.
            if (string.IsNullOrEmpty(packageFolder))
            {
                // File goes to the determined target path (or the root of the package), such as a readme.txt
                packagePath = targetPath;
            }
            else
            {
                if (frameworkSpecific)
                {
                    // For (framework-specific) tools, we need to append 'any' at the end
                    if (packageFolder == PackagingConstants.Folders.Tools)
                        packagePath = Path.Combine(new[] { packageFolder, targetFramework, "any" }.Concat(targetPath.Split(Path.DirectorySeparatorChar)).ToArray());
                    else
                        packagePath = Path.Combine(new[] { packageFolder, targetFramework }.Concat(targetPath.Split(Path.DirectorySeparatorChar)).ToArray());
                }
                else
                {
                    packagePath = Path.Combine(new[] { packageFolder }.Concat(targetPath.Split(Path.DirectorySeparatorChar)).ToArray());
                }
            }

            output.SetMetadata(MetadataName.PackagePath, packagePath.Replace('\\', '/'));

            return output;
        }
    }
}
