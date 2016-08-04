using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NuGet.Common;
using NuGet.Packaging.Core;
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

            string projectJsonPath = Path.Combine(projectDirectory, ProjectJsonPathUtilities.ProjectConfigFileName);
            if (File.Exists (projectJsonPath))
            {
                // If there is a packages.config file and a project.json file then
                // the project.json should be used.
                return null;
            }

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
            var item = packageReference.CreatePackageReferenceTaskItem(project);

            var packageDirectoryPath = GetPackageDirectoryPath(project.GetMetadata("FullPath"), packageReference.PackageIdentity);
            item.SetMetadata("PackageDirectoryPath", packageDirectoryPath);

            return item;
        }

        private static string GetPackageDirectoryPath(string packagesConfigPath, PackageIdentity identity)
        {
            var packageDirectoryName = identity.Id + "." + identity.Version;
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