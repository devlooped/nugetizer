using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Packaging.Licenses;
using NuGet.Versioning;
using static ThisAssembly.Strings;

namespace NuGetizer.Tasks
{
    public class CreatePackage : Task
    {
        [Required]
        public ITaskItem Manifest { get; set; }

        [Required]
        public ITaskItem[] Contents { get; set; } = Array.Empty<ITaskItem>();

        [Required]
        public string TargetPath { get; set; }

        public string NuspecFile { get; set; }

        [Output]
        public ITaskItem OutputPackage { get; set; }

        public override bool Execute()
        {
            try
            {
                using (var stream = File.Create(TargetPath))
                {
                    BuildPackage(stream);
                }

                OutputPackage = new TaskItem(TargetPath);
                Manifest.CopyMetadataTo(OutputPackage);

                return !Log.HasLoggedErrors;
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
                return false;
            }
        }

        // Implementation for testing to avoid I/O
        public Manifest Execute(Stream output)
        {
            BuildPackage(output);

            output.Seek(0, SeekOrigin.Begin);
            using (var reader = new PackageArchiveReader(output))
            {
                return reader.GetManifest();
            }
        }

        public Manifest CreateManifest()
        {
            var metadata = new ManifestMetadata();

            metadata.Id = Manifest.GetMetadata(nameof(ManifestMetadata.Id));

            if (Manifest.TryGetMetadata(nameof(ManifestMetadata.Version), out var version))
                metadata.Version = NuGetVersion.Parse(Manifest.GetMetadata(MetadataName.Version));

            if (Manifest.TryGetBoolMetadata(nameof(ManifestMetadata.DevelopmentDependency), out var devDep) && devDep)
                metadata.DevelopmentDependency = true;

            metadata.Title = Manifest.GetMetadata("Title");
            metadata.Description = Manifest.GetMetadata("Description");
            metadata.Summary = Manifest.GetMetadata("Summary");
            metadata.Language = Manifest.GetMetadata("Language");

            metadata.Copyright = Manifest.GetMetadata("Copyright");
            metadata.RequireLicenseAcceptance = Manifest.GetBoolean("RequireLicenseAcceptance");

            if (!string.IsNullOrEmpty(Manifest.GetMetadata("Authors")))
                metadata.Authors = Manifest.GetMetadata("Authors").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (!string.IsNullOrEmpty(Manifest.GetMetadata("Owners")))
                metadata.Owners = Manifest.GetMetadata("Owners").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (!string.IsNullOrEmpty(Manifest.GetMetadata("LicenseUrl")))
                metadata.SetLicenseUrl(Manifest.GetMetadata("LicenseUrl"));

            if (Manifest.TryGetMetadata("LicenseExpression", out var expression))
                metadata.LicenseMetadata = new LicenseMetadata(
                    LicenseType.Expression,
                    expression,
                    NuGetLicenseExpression.Parse(expression),
                    null, LicenseMetadata.CurrentVersion);

            if (!string.IsNullOrEmpty(Manifest.GetMetadata("ProjectUrl")))
                metadata.SetProjectUrl(Manifest.GetMetadata("ProjectUrl"));

            if (Manifest.TryGetMetadata("RepositoryType", out var repoType))
                (metadata.Repository ??= new RepositoryMetadata()).Type = repoType;

            if (Manifest.TryGetMetadata("RepositoryUrl", out var repoUrl))
                (metadata.Repository ??= new RepositoryMetadata()).Url = repoUrl;

            if (Manifest.TryGetMetadata("RepositoryBranch", out var repoBranch))
                (metadata.Repository ??= new RepositoryMetadata()).Branch = repoBranch;

            if (Manifest.TryGetMetadata("RepositoryCommit", out var repoCommit))
                (metadata.Repository ??= new RepositoryMetadata()).Commit = repoCommit;

            if (!string.IsNullOrEmpty(Manifest.GetMetadata("ProjectUrl")))
                metadata.SetProjectUrl(Manifest.GetMetadata("ProjectUrl"));

            if (!string.IsNullOrEmpty(Manifest.GetMetadata("ProjectUrl")))
                metadata.SetProjectUrl(Manifest.GetMetadata("ProjectUrl"));

            if (!string.IsNullOrEmpty(Manifest.GetMetadata("IconUrl")))
                metadata.SetIconUrl(Manifest.GetMetadata("IconUrl"));

            if (Manifest.TryGetMetadata("Icon", out var icon))
                metadata.Icon = icon;

            metadata.ReleaseNotes = Manifest.GetMetadata("ReleaseNotes");
            metadata.Tags = Manifest.GetMetadata("Tags");
            metadata.MinClientVersionString = Manifest.GetMetadata("MinClientVersion");
            metadata.PackageTypes = ParsePackageTypes(Manifest.GetMetadata("PackageTypes"));

            var manifest = new Manifest(metadata);

            AddDependencies(manifest);
            AddFiles(manifest);
            AddFrameworkAssemblies(manifest);

            return manifest;
        }

        void AddDependencies(Manifest manifest)
        {
            var dependencies = from item in Contents
                               where item.GetMetadata(MetadataName.Kind) == PackageItemKind.Dependency &&
                                     !"all".Equals(item.GetMetadata(MetadataName.PrivateAssets), StringComparison.OrdinalIgnoreCase)
                               select new Dependency
                               {
                                   Id = item.ItemSpec,
                                   Version = VersionRange.Parse(item.GetMetadata(MetadataName.Version)),
                                   TargetFramework = item.GetNuGetTargetFramework(),
                                   Include = item.GetNullableMetadata(MetadataName.IncludeAssets),
                                   Exclude = item.GetNullableMetadata(MetadataName.ExcludeAssets)
                               };

            var definedDependencyGroups = (from dependency in dependencies
                                           group dependency by dependency.TargetFramework into dependenciesByFramework
                                           select new PackageDependencyGroup
                                           (
                                               dependenciesByFramework.Key,
                                               (from dependency in dependenciesByFramework
                                                where dependency.Id != "_._"
                                                group dependency by dependency.Id into dependenciesById
                                                select new PackageDependency
                                                 (
                                                     dependenciesById.Key,
                                                     dependenciesById.Select(x => x.Version).Aggregate(AggregateVersions),
                                                     dependenciesById.Select(x => x.Include).Aggregate(default(List<string>), AggregateAssetsFlow),
                                                     dependenciesById.Select(x => x.Exclude).Aggregate(default(List<string>), AggregateAssetsFlow)
                                                 )).ToList()
                                           )).ToDictionary(p => p.TargetFramework.GetFrameworkString());

            // include frameworks referenced by libraries, but without dependencies..
            foreach (var targetFramework in (from item in Contents
                                             where item.GetMetadata(MetadataName.Kind) == PackageItemKind.Lib &&
                                                   !"all".Equals(item.GetMetadata(MetadataName.PrivateAssets), StringComparison.OrdinalIgnoreCase)
                                             select item.GetNuGetTargetFramework()))
                if (!definedDependencyGroups.ContainsKey(targetFramework.GetFrameworkString()))
                    definedDependencyGroups.Add(targetFramework.GetFrameworkString(),
                                                new PackageDependencyGroup(targetFramework, Array.Empty<PackageDependency>()));

            manifest.Metadata.DependencyGroups = definedDependencyGroups.Values;
        }

        void AddFiles(Manifest manifest)
        {
            var contents = new List<ITaskItem>();

            var groupedByPackagePath = Contents
                .Where(item => !string.IsNullOrEmpty(item.GetMetadata(MetadataName.PackagePath)))
                .GroupBy(item => item.GetMetadata(MetadataName.PackagePath))
                // Iterate only once for this grouping.
                .ToDictionary(item => item.Key, item => item.ToArray());

            // Add the ones we already determined as unique by package path.
            contents.AddRange(groupedByPackagePath
                .Where(group => group.Value.Length == 1)
                .Select(group => group.Value.First()));

            var groupedByLastWriteAndLength = groupedByPackagePath
                .Where(group => group.Value.Length > 1)
                .SelectMany(group => group.Value)
                // Tuple provides structural comparison and hashing already, so leverage that.
                .GroupBy(item => Tuple.Create(
                    item.GetMetadata(MetadataName.PackagePath),
                    item.GetMetadata("Filename"),
                    item.GetMetadata("Extension"),
                    File.GetLastWriteTime(item.GetMetadata("FullPath")),
                    new FileInfo(item.GetMetadata("FullPath")).Length))
                .ToDictionary(item => item.Key, item => item.ToArray());

            // Add the ones we already determined to be duplicates that can safely be 
            // unified by package path, file name, last write time and file length.
            contents.AddRange(groupedByLastWriteAndLength
                .Where(group => group.Value.Length > 1)
                .Select(group => group.Value.First()));

            var md5 = new Lazy<HashAlgorithm>(() => MD5.Create());
            string hash(ITaskItem item)
            {
                using (var file = File.OpenRead(item.GetMetadata("FullPath")))
                {
                    return string.Concat(md5.Value.ComputeHash(file).Select(x => x.ToString("x2")));
                }
            }

            // Last remaining attempt at de-duplication is costly, but by now, we should 
            // have successfully removed all obvious cases. 
            // This deals with case where the files are modified at different times 
            // (maybe a generated file?) but their actual contents are the same.
            var groupedByContentHash = groupedByLastWriteAndLength
                .Where(group => group.Value.Length == 1)
                .SelectMany(group => group.Value)
                .GroupBy(item => Tuple.Create(
                    item.GetMetadata(MetadataName.PackagePath),
                    hash(item)))
                .ToDictionary(item => item.Key, item => item.ToArray());

            // Add the ones we determined to be duplicates that can safely be 
            // unified by package path and MD5 hash
            contents.AddRange(groupedByContentHash
                .Where(group => group.Value.Length > 1)
                .Select(group => group.Value.First()));

            // At this point, we're 100% certain these are duplicate package path 
            // files that have distinct sources and would result in one overwriting 
            // the other or an invalid package.
            var duplicates = string.Join(Environment.NewLine, groupedByContentHash
                .Where(group => group.Value.Length == 1)
                .SelectMany(group => group.Value)
                .Select(item => $"'{item.GetMetadata("FullPath")}' > '{item.GetMetadata(MetadataName.PackagePath)}'"));

            if (duplicates.Length > 0)
                Log.LogErrorCode(nameof(ErrorCode.NG0012), ErrorCode.NG0012(duplicates));

            // All files need to be added so they are included in the nupkg
            manifest.Files.AddRange(contents
                .Select(item => new ManifestFile
                {
                    Source = item.GetMetadata("FullPath"),
                    Target = item.GetMetadata(MetadataName.PackagePath),
                }));

            // Additional metadata for the content files must be added separately
            manifest.Metadata.ContentFiles = contents
                .Where(item => item.GetMetadata(MetadataName.PackageFolder) == PackagingConstants.Folders.ContentFiles)
                .Select(item => new ManifestContentFiles
                {
                    Include = item.GetContentFileInclude(),
                    BuildAction = item.GetNullableMetadata(MetadataName.ContentFile.BuildAction),
                    CopyToOutput = item.GetNullableMetadata(MetadataName.ContentFile.CopyToOutput),
                    Flatten = item.GetNullableMetadata(MetadataName.ContentFile.Flatten),
                }).ToArray();
        }

        void AddFrameworkAssemblies(Manifest manifest)
        {
            var frameworkReferences = (from item in Contents
                                       where item.GetMetadata(MetadataName.Kind) == PackageItemKind.FrameworkReference
                                       select new FrameworkAssemblyReference
                                      (
                                          item.ItemSpec,
                                          new[] { NuGetFramework.Parse(item.GetTargetFrameworkMoniker().FullName) }
                                      )).Distinct(FrameworkAssemblyReferenceComparer.Default);

            manifest.Metadata.FrameworkReferences = frameworkReferences;
        }

        void BuildPackage(Stream output)
        {
            var builder = new PackageBuilder();
            var manifest = CreateManifest();

            builder.Populate(manifest.Metadata);
            // We don't use PopulateFiles because that performs search expansion, base path 
            // extraction and the like, which messes with our determined files to include.
            // TBD: do we support wilcard-based include/exclude?
            builder.Files.AddRange(manifest.Files.Select(file =>
                new PhysicalPackageFile { SourcePath = file.Source, TargetPath = file.Target }));

            builder.Save(output);

            if (!string.IsNullOrEmpty(NuspecFile))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(NuspecFile));
                using (var stream = File.Create(NuspecFile))
                {
                    manifest.Save(stream, true);
                }
            }
        }

        static VersionRange AggregateVersions(VersionRange aggregate, VersionRange next)
        {
            var versionSpec = new VersionSpec();
            SetMinVersion(versionSpec, aggregate);
            SetMinVersion(versionSpec, next);
            SetMaxVersion(versionSpec, aggregate);
            SetMaxVersion(versionSpec, next);

            if (versionSpec.MinVersion == null && versionSpec.MaxVersion == null)
                return null;

            return versionSpec.ToVersionRange();
        }

        static List<string> AggregateAssetsFlow(List<string> aggregate, string next)
        {
            if (next == null)
                return aggregate;
            if (aggregate == null)
                aggregate = new List<string>(1);
            aggregate.AddRange(next.Split(';'));
            return aggregate;
        }

        static void SetMinVersion(VersionSpec target, VersionRange source)
        {
            if (source == null || source.MinVersion == null)
                return;

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
                target.IsMinInclusive = target.IsMinInclusive && source.IsMinInclusive;
        }

        static void SetMaxVersion(VersionSpec target, VersionRange source)
        {
            if (source == null || source.MaxVersion == null)
                return;

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
                target.IsMaxInclusive = target.IsMaxInclusive && source.IsMaxInclusive;
        }

        static ICollection<PackageType> ParsePackageTypes(string packageTypes)
        {
            var listOfPackageTypes = new List<PackageType>();
            if (!string.IsNullOrEmpty(packageTypes))
            {
                foreach (var packageType in packageTypes.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] packageTypeSplitInPart = packageType.Split(new char[] { ',' });
                    string packageTypeName = packageTypeSplitInPart[0].Trim();
                    var version = PackageType.EmptyVersion;
                    if (packageTypeSplitInPart.Length > 1)
                    {
                        string versionString = packageTypeSplitInPart[1];
                        Version.TryParse(versionString, out version);
                    }
                    listOfPackageTypes.Add(new PackageType(packageTypeName, version));
                }
            }
            return listOfPackageTypes;
        }

        class Dependency
        {
            public string Id { get; set; }

            public NuGetFramework TargetFramework { get; set; }

            public VersionRange Version { get; set; }

            public string Include { get; set; }

            public string Exclude { get; set; }
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
