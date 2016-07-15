using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;

namespace NuGet.Packaging.Tasks
{
    public class ReadProjectJson : Task
    {
        [Required]
        public ITaskItem[] Projects { get; set; }

        [Required]
        public string TargetFrameworkMoniker { get; set; }

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

        IEnumerable<PackageReference> GetPackageReferences(string projectFileFullPath)
        {
            var packages = new List<PackageReference>();

            string projectDirectory = Path.GetDirectoryName(projectFileFullPath);
            string projectJsonFile = Path.Combine(projectDirectory, ProjectJsonPathUtilities.ProjectConfigFileName);

            if (!File.Exists(projectJsonFile))
            {
                return packages;
            }

            var framework = NuGetFramework.Parse(TargetFrameworkMoniker);

            foreach (var dependency in JsonConfigUtility.GetDependencies(GetJson(projectJsonFile)))
            {
                // Use the minimum version of the range for the identity
                var identity = new PackageIdentity(dependency.Id, dependency.VersionRange.MinVersion);

                // Pass the actual version range as the allowed range
                packages.Add(new PackageReference(identity,
                    targetFramework: framework,
                    userInstalled: true,
                    developmentDependency: false,
                    requireReinstallation: false,
                    allowedVersions: dependency.VersionRange));
             }

             return packages;
        }

        static JObject GetJson(string projectJsonFile)
        {
            using (var streamReader = new StreamReader(projectJsonFile))
            {
                return JObject.Parse(streamReader.ReadToEnd());
            }
        }

        protected ITaskItem ConvertPackageElement(ITaskItem project, PackageReference packageReference)
        {
            var item = packageReference.CreatePackageReferenceTaskItem(project);

            var packageDirectoryPath = GetPackageDirectoryPath(packageReference.PackageIdentity);
            item.SetMetadata("PackageDirectoryPath", packageDirectoryPath);

            return item;
        }

        static string GetPackageDirectoryPath(PackageIdentity identity)
        {
            string packagesFolderPath = SettingsUtility.GetGlobalPackagesFolder(Configuration.Settings.LoadDefaultSettings (null, null, null));
            var packagePathResolver = new VersionFolderPathResolver(packagesFolderPath, normalizePackageId: false);
            return packagePathResolver.GetInstallPath(identity.Id, identity.Version);
        }
    }
}