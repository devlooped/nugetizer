using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NuGet.Versioning;

namespace NuGet.Packaging.Tasks
{
    public class ReadPackagesConfig : Task
    {
        [Required]
        public ITaskItem[] Projects { get; set; }

        [Output]
        public ITaskItem[] PackageReferences { get; set; }

        public override bool Execute()
        {
            var packageReferences = from project in Projects
                                    from packageReference in GetPackageReferences(project.GetMetadata("FullPath"))
                                    select ConvertPackageElement(project, packageReference);
            PackageReferences = packageReferences.ToArray();
            return true;
        }

        static IEnumerable<PackageReference> GetPackageReferences(string projectFileFullPath)
        {
            var xmlDocument = ReadPackagesConfigFile(projectFileFullPath);

            if (xmlDocument != null)
            {
                var reader = new PackagesConfigReader(xmlDocument);
                return reader.GetPackages().ToList();
            }

            return new List<PackageReference>();
        }

        static XDocument ReadPackagesConfigFile(string projectFileFullPath)
        {
            string projectName = Path.GetFileNameWithoutExtension(projectFileFullPath);
            string projectDirectory = Path.GetDirectoryName(projectFileFullPath);
            string packagesConfigPath = Path.Combine(projectDirectory, "packages.config");
            string packagesProjectNameConfigPath = Path.Combine(projectDirectory, "packages." + projectName + ".config");

            if (File.Exists(packagesProjectNameConfigPath))
            {
                packagesConfigPath = packagesProjectNameConfigPath;
            }

            if (File.Exists (packagesConfigPath))
            {
                return XDocument.Load(packagesConfigPath);
            }

            return null;
        }

        protected ITaskItem ConvertPackageElement(ITaskItem project, PackageReference packageReference)
        {
            var id = packageReference.PackageIdentity.Id;
            var version = packageReference.PackageIdentity.Version;
            var targetFramework = packageReference.TargetFramework;
            var isDevelopmentDependency = packageReference.IsDevelopmentDependency;
            var requireReinstallation = packageReference.RequireReinstallation;
            var versionConstraint = packageReference.AllowedVersions;

            var item = new TaskItem(id);
            project.CopyMetadataTo(item);

            var packageDirectoryPath = GetPackageDirectoryPath(project.GetMetadata("FullPath"), id, version);
            item.SetMetadata("PackageDirectoryPath", packageDirectoryPath);
            item.SetMetadata("ProjectPath", project.GetMetadata("FullPath"));

            item.SetMetadata("IsDevelopmentDependency", isDevelopmentDependency.ToString());
            item.SetMetadata("RequireReinstallation", requireReinstallation.ToString());

            if (version != null)
                item.SetMetadata(Metadata.Version, version.ToString());

            if (targetFramework != null)
                item.SetMetadata(Metadata.TargetFramework, targetFramework.GetShortFolderName());

            if (versionConstraint != null)
                item.SetMetadata("VersionConstraint", versionConstraint.ToString());

            return item;
        }

        private static string GetPackageDirectoryPath(string packagesConfigPath, string packageId, NuGetVersion packageVersion)
        {
            var packageDirectoryName = packageId + "." + packageVersion;
            var candidateFolder = Path.GetDirectoryName(packagesConfigPath);
            while (candidateFolder != null)
            {
                var packageDirectoryPath = Path.Combine(candidateFolder, "packages", packageDirectoryName);
                if (Directory.Exists(packageDirectoryPath))
                    return packageDirectoryPath;

                candidateFolder = Path.GetDirectoryName(candidateFolder);
            }

            return string.Empty;
        }
    }
}