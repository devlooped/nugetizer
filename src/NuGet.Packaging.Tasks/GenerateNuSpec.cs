using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using NuGet.Versioning;

namespace NuGet.Packaging.Tasks
{
    public class GenerateNuSpec : Task
    {
        private const string NuSpecXmlNamespace = @"http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd";
        private const string NuGetPackageIdPlaceholder = "NUGET_PACKAGE_ID_PLACEHOLDER";

        public string InputFileName { get; set; }

        [Required]
        public string OutputFileName { get; set; }

        public string MinClientVersion { get; set; }

        [Required]
        public string Id { get; set; }

        [Required]
        public string Version { get; set; }

        public string Title { get; set; }

        [Required]
        public string Authors { get; set; }

        public string Owners { get; set; }

        [Required]
        public string Description { get; set; }

        public string ReleaseNotes { get; set; }

        public string Summary { get; set; }

        public string Language { get; set; }

        public string ProjectUrl { get; set; }

        public string IconUrl { get; set; }

        public string LicenseUrl { get; set; }

        public string Copyright { get; set; }

        public bool RequireLicenseAcceptance { get; set; }

        public bool DevelopmentDependency { get; set; }

        public string Tags { get; set; }

        public ITaskItem[] Dependencies { get; set; }

        public ITaskItem[] References { get; set; }

        public ITaskItem[] FrameworkReferences { get; set; }

        public ITaskItem[] Files { get; set; }

        public ITaskItem[] NuSpecFileDependencies { get; set; }

        [Required]
        public string TargetFrameworkMoniker { get; set; }

        [Output]
        public ITaskItem[] FilesWritten { get; set; }

        public override bool Execute()
        {
            try
            {
                WriteNuSpecFile();
            }
            catch (Exception ex)
            {
                Log.LogError(ex.ToString());
                Log.LogErrorFromException(ex);
            }

            return !Log.HasLoggedErrors;
        }

        private void WriteNuSpecFile()
        {
            var manifest = CreateManifest();

            if (!IsDifferent(manifest))
            {
                Log.LogMessage("Skipping generation of .nuspec because contents are identical.");
                FilesWritten = new[] { new TaskItem(OutputFileName) };
                return;
            }

            var directory = Path.GetDirectoryName(OutputFileName);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var file = File.Create(OutputFileName))
            {
                manifest.Save(file, false);
                FilesWritten = new[] { new TaskItem(OutputFileName) };
            }
        }

        private bool IsDifferent(Manifest newManifest)
        {
            if (!File.Exists(OutputFileName))
                return true;

            var oldSource = File.ReadAllText(OutputFileName);
            var newSource = "";
            using (var stream = new MemoryStream())
            {
                newManifest.Save(stream, false);
                stream.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(stream, Encoding.UTF8)) {
                    newSource = reader.ReadToEnd();
                }
            }

            return oldSource != newSource;
        }

        private Manifest CreateManifest()
        {
            Manifest manifest;
            ManifestMetadata manifestMetadata;
            if (!string.IsNullOrEmpty(InputFileName))
            {
                using (var stream = File.OpenRead(InputFileName))
                {
                    manifest = Manifest.ReadFrom(stream, false);
                }
            }
            else
            {
                manifest = new Manifest(new ManifestMetadata());
            }

            manifestMetadata = manifest.Metadata;

            if (!String.IsNullOrEmpty(Authors))
                manifestMetadata.Authors = SplitCommaSeparatedString(Authors);
            manifestMetadata.UpdateMember(x => x.Copyright, Copyright);
            manifestMetadata.AddRangeToMember(x => x.DependencyGroups, GetDependencySets());
            manifestMetadata.UpdateMember(x => x.Description, Description);
            manifestMetadata.DevelopmentDependency |= DevelopmentDependency;
            manifestMetadata.AddRangeToMember(x => x.FrameworkReferences, GetFrameworkAssemblies());
            if (!String.IsNullOrEmpty(IconUrl))
                manifestMetadata.SetIconUrl(IconUrl);
            manifestMetadata.UpdateMember(x => x.Id, Id);
            manifestMetadata.UpdateMember(x => x.Language, Language);
            if (!String.IsNullOrEmpty(LicenseUrl))
                manifestMetadata.SetLicenseUrl(LicenseUrl);
            manifestMetadata.UpdateMember(x => x.MinClientVersionString, MinClientVersion);
            if (!String.IsNullOrEmpty(Owners))
                manifestMetadata.Owners = SplitCommaSeparatedString(Owners);
            if (!String.IsNullOrEmpty(ProjectUrl))
                manifestMetadata.SetProjectUrl(ProjectUrl);
            manifestMetadata.AddRangeToMember(x => x.PackageAssemblyReferences, GetReferenceSets());
            manifestMetadata.UpdateMember(x => x.ReleaseNotes, ReleaseNotes);
            manifestMetadata.RequireLicenseAcceptance |= RequireLicenseAcceptance;
            manifestMetadata.UpdateMember(x => x.Summary, Summary);
            manifestMetadata.UpdateMember(x => x.Tags, Tags);
            manifestMetadata.UpdateMember(x => x.Title, Title);
            if (!String.IsNullOrEmpty(Version))
                manifestMetadata.Version = GetNuGetVersion(Version);

            manifest.Files.AddRange(GetManifestFiles());

            AddNuSpecFileDependencies(manifest);

            return manifest;
        }

        void AddNuSpecFileDependencies(Manifest manifest)
        {
            foreach (ITaskItem nuspecFileDependency in NuSpecFileDependencies.NullAsEmpty())
            {
                string fileName = nuspecFileDependency.GetMetadata(Metadata.FileSource);
                using (var stream = File.OpenRead(fileName))
                {
                    var dependencyManifest = Manifest.ReadFrom(stream, false);
                    if (dependencyManifest.Metadata.Id == NuGetPackageIdPlaceholder)
                    {
                        MergeDependencyManifest(manifest, dependencyManifest);
                    }
                    else
                    {
                        RemoveDependencyManifestFiles(manifest, dependencyManifest);
                        AddNuGetDependency(manifest, dependencyManifest);
                    }
                }
            }
        }

        void MergeDependencyManifest(Manifest manifest, Manifest dependencyManifest)
        {
            var manifestMetadata = manifest.Metadata;
            var dependencyManifestMetadata = dependencyManifest.Metadata;

            MergeFiles(manifest, dependencyManifest);
            MergeDependencyGroups(manifestMetadata, dependencyManifestMetadata);
            MergeFrameworkReferences(manifestMetadata, dependencyManifestMetadata);
            MergePackageAssemblyReferences(manifestMetadata, dependencyManifestMetadata);
        }

        static void MergeFiles(Manifest manifest, Manifest dependencyManifest)
        {
            var filesToAdd = dependencyManifest
                .Files
                .Where(file => !manifest.Files.Any(item => item.Source == file.Source))
                .ToList();
            
            manifest.Files.AddRange(filesToAdd);
        }

        void MergeDependencyGroups(ManifestMetadata manifestMetadata, ManifestMetadata dependencyManifestMetadata)
        {
            if (!dependencyManifestMetadata.DependencyGroups.Any())
                return;

            manifestMetadata.DependencyGroups = Merge(
                manifestMetadata.DependencyGroups,
                dependencyManifestMetadata.DependencyGroups,
                dependencyGroup => dependencyGroup.TargetFramework,
                dependencyGroup => dependencyGroup.Packages,
                (x, y) => x.Equals(y),
                CreateDependencyGroup);
        }

        PackageDependencyGroup CreateDependencyGroup(PackageDependencyGroup dependencyGroup, IList<Core.PackageDependency> packages)
        {
            var updatedPackages = new List<Core.PackageDependency>();

            foreach (Core.PackageDependency package in dependencyGroup.Packages)
            {
                var existingPackage = packages.FirstOrDefault(
                    p => p.Id.Equals(package.Id, StringComparison.OrdinalIgnoreCase));

                if (existingPackage == null)
                {
                    updatedPackages.Add(package);
                }
                else
                {
                    LogWarning("NuGet package dependency with different version exists. Taking highest version. {0}, {1}",
                        package, existingPackage);

                    if (existingPackage.IsOlderVersionThan(package))
                    {
                        updatedPackages.Add(package);
                        packages.Remove(existingPackage);
                    }
                }
            }

            updatedPackages.AddRange(packages);

            return new PackageDependencyGroup(
                dependencyGroup.TargetFramework,
                updatedPackages);
        }

        protected virtual void LogWarning(string message, params object[] messageArgs)
        {
            Log.LogWarning(message, messageArgs);
        }

        static void MergeFrameworkReferences(ManifestMetadata manifestMetadata, ManifestMetadata dependencyManifestMetadata)
        {
            var frameworkReferencesToAdd = dependencyManifestMetadata
                .FrameworkReferences
                .Where(frameworkReference => !manifestMetadata.FrameworkReferences.Any(
                    item => item.AssemblyName == frameworkReference.AssemblyName
                    && item.SupportedFrameworks.Single().Equals(frameworkReference.SupportedFrameworks.Single())))
                .ToList();

            manifestMetadata.AddRangeToMember(x => x.FrameworkReferences, frameworkReferencesToAdd);
        }

        static void MergePackageAssemblyReferences(ManifestMetadata manifestMetadata, ManifestMetadata dependencyManifestMetadata)
        {
            if (!dependencyManifestMetadata.PackageAssemblyReferences.Any())
                return;

            manifestMetadata.PackageAssemblyReferences = Merge(
                manifestMetadata.PackageAssemblyReferences,
                dependencyManifestMetadata.PackageAssemblyReferences,
                referenceSet => referenceSet.TargetFramework,
                referenceSet => referenceSet.References,
                (x, y) => string.Equals(x, y, StringComparison.OrdinalIgnoreCase),
                CreateReferenceSet);
        }

        static IEnumerable<G> Merge<T, G>(
            IEnumerable<G> group1,
            IEnumerable<G> group2,
            Func<G, NuGetFramework> getTargetFramework,
            Func<G, IEnumerable<T>> getItems,
            Func<T, T, bool> isMatch,
            Func<G, List<T>, G> createGroup)
            where T : class
            where G : class
        {
            var mergedGroups = new List<G>();

            foreach (G currentGroup in group2)
            {
                G existingGroup = group1.FirstOrDefault(
                    item => getTargetFramework(item).Equals(getTargetFramework(currentGroup)));

                if (existingGroup == null)
                {
                    mergedGroups.Add(currentGroup);
                }
                else
                {
                    List<T> items = getItems(currentGroup)
                        .Where(item => !getItems(existingGroup).Any(existingItem => isMatch(existingItem, item)))
                        .ToList();

                    if (items.Any())
                    {
                        mergedGroups.Add(createGroup(existingGroup, items));
                    }
                }
            }

            IEnumerable<G> existingGroups = group1.Where(
                existingGroup => !mergedGroups.Any(
                    currentGroup => getTargetFramework(currentGroup).Equals(getTargetFramework(existingGroup))));

            mergedGroups.AddRange(existingGroups);

            return mergedGroups;
        }

        static PackageReferenceSet CreateReferenceSet(PackageReferenceSet referenceSet, IList<string> references)
        {
            references.AddRange(referenceSet.References);

            return new PackageReferenceSet(
                referenceSet.TargetFramework,
                references);
        }

        /// <summary>
        /// Removes any files that belong to another project's manifest from the current
        /// project's manifest.
        /// </summary>
        void RemoveDependencyManifestFiles(Manifest manifest, Manifest dependencyManifest)
        {
            foreach (ManifestFile fileToRemove in dependencyManifest.Files)
            {
                manifest.Files.RemoveAll(file => file.Source == fileToRemove.Source);
            }
         }

        void AddNuGetDependency(Manifest manifest, Manifest dependencyManifest)
        {
            var manifestMetadata = manifest.Metadata;
            var dependencyManifestMetadata = dependencyManifest.Metadata;

            var packageDependency = new Core.PackageDependency(
                dependencyManifestMetadata.Id,
                new VersionRange(dependencyManifestMetadata.Version));

            var dependencyGroup = new PackageDependencyGroup(
                NuGetFramework.Parse(TargetFrameworkMoniker),
                new [] { packageDependency });
            var dependencyGroups = new List<PackageDependencyGroup>();
            dependencyGroups.Add(dependencyGroup);

            manifestMetadata.AddRangeToMember(x => x.DependencyGroups, dependencyGroups);
        }

        IEnumerable<string> SplitCommaSeparatedString(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            return value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }

        NuGetVersion GetNuGetVersion(string version)
        {
            if (string.IsNullOrEmpty(version))
                return null;

            return new NuGetVersion(version);
        }

        private List<ManifestFile> GetManifestFiles()
        {
            return (from f in Files.NullAsEmpty()
                    select new ManifestFile
                    {
                        Source = f.GetMetadata(Metadata.FileSource),
                        Target = f.GetMetadataAsPath(Metadata.FileTarget),
                        Exclude = f.GetMetadata(Metadata.FileExclude),
                    }).ToList();
        }

        private List<FrameworkAssemblyReference> GetFrameworkAssemblies()
        {
            return (from fr in FrameworkReferences.NullAsEmpty()
                    select new FrameworkAssemblyReference
                    (
                        fr.ItemSpec,
                        new [] { NuGetFramework.Parse(fr.GetTargetFramework().GetShortFrameworkName()) }
                    )).ToList();
        }

        private List<PackageDependencyGroup> GetDependencySets()
        {
            var dependencies = from d in Dependencies.NullAsEmpty()
                               select new Dependency
                               {
                                   Id = d.ItemSpec,
                                   Version = d.GetVersion(),
                                   TargetFramework = d.GetTargetFramework()
                               };

            return (from dependency in dependencies
                    group dependency by dependency.TargetFramework into dependenciesByFramework
                    select new PackageDependencyGroup
                    (
                        NuGetFramework.Parse(dependenciesByFramework.Key.GetShortFrameworkName() ?? "any"),
                        (from dependency in dependenciesByFramework
                         where dependency.Id != "_._"
                         group dependency by dependency.Id into dependenciesById
                         select new Core.PackageDependency
                         (
                             dependenciesById.Key,
                             dependenciesById.Select(x => x.Version)
                             .Aggregate(AggregateVersions)
                         )).ToList()
                    )).ToList();
        }

        private List<PackageReferenceSet> GetReferenceSets()
        {
            var references = from r in References.NullAsEmpty()
                             select new
                             {
                                 File = r.ItemSpec,
                                 TargetFramework = r.GetTargetFramework(),
                             };

            return (from reference in references
                    group reference by reference.TargetFramework into referencesByFramework
                    select new PackageReferenceSet
                    (
                        referencesByFramework.Key.GetShortFrameworkName(),
                        (from reference in referencesByFramework
                         select reference.File
                        ).ToList()
                    )).ToList();
        }

        private static VersionRange AggregateVersions(VersionRange aggregate, VersionRange next)
        {
            var versionSpec = new VersionSpec();
            SetMinVersion(versionSpec, aggregate);
            SetMinVersion(versionSpec, next);
            SetMaxVersion(versionSpec, aggregate);
            SetMaxVersion(versionSpec, next);

            if (versionSpec.MinVersion == null && versionSpec.MaxVersion == null)
            {
                return null;
            }

            return versionSpec.ToVersionRange();
        }

        private static void SetMinVersion(VersionSpec target, VersionRange source)
        {
            if (source == null || source.MinVersion == null)
            {
                return;
            }

            if (target.MinVersion == null)
            {
                target.MinVersion = source.MinVersion;
                target.IsMinInclusive = source.IsMinInclusive;
            }

            if (target.MinVersion < source.MinVersion)
            {
                target.MinVersion = source.MinVersion;
                target.IsMinInclusive = source.IsMinInclusive;
            }

            if (target.MinVersion == source.MinVersion)
            {
                target.IsMinInclusive = target.IsMinInclusive && source.IsMinInclusive;
            }
        }

        private static void SetMaxVersion(VersionSpec target, VersionRange source)
        {
            if (source == null || source.MaxVersion == null)
            {
                return;
            }

            if (target.MaxVersion == null)
            {
                target.MaxVersion = source.MaxVersion;
                target.IsMaxInclusive = source.IsMaxInclusive;
            }

            if (target.MaxVersion > source.MaxVersion)
            {
                target.MaxVersion = source.MaxVersion;
                target.IsMaxInclusive = source.IsMaxInclusive;
            }

            if (target.MaxVersion == source.MaxVersion)
            {
                target.IsMaxInclusive = target.IsMaxInclusive && source.IsMaxInclusive;
            }
        }

        private class Dependency
        {
            public string Id { get; set; }

            public FrameworkName TargetFramework { get; set; }

            public VersionRange Version { get; set; }
        }

        class VersionSpec
        {
            public bool IsMinInclusive { get; set; }
            public NuGetVersion MinVersion { get; set; }
            public bool IsMaxInclusive { get; set; }
            public NuGetVersion MaxVersion { get; set; }

            public VersionRange ToVersionRange()
            {
                return new VersionRange(MinVersion, IsMinInclusive, MaxVersion, IsMaxInclusive);
            }
        }
    }
}