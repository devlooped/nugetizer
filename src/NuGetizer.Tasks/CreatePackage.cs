using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
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
        public ITaskItem[] Contents { get; set; } = [];

        public ITaskItem[] ReplacementTokens { get; set; } = [];

        [Required]
        public string TargetPath { get; set; }

        [Required]
        public string EmitPackage { get; set; }

        [Required]
        public string EmitNuspec { get; set; }

        [Output]
        public string NuspecFile { get; set; }

        [Output]
        public ITaskItem OutputPackage { get; set; }

        Manifest manifest;
        Dictionary<string, string> tokens;
        Regex tokensExpr;
        Regex linkExpr;

        public override bool Execute()
        {
            if (Environment.GetEnvironmentVariable("DEBUG_NUGETIZER") == "1" ||
                Environment.GetEnvironmentVariable("DEBUG_NUGETIZER_PACK") == "1")
                Debugger.Launch();

            try
            {
                if (bool.TryParse(EmitPackage, out var emitPkg) && emitPkg)
                    GeneratePackage();

                if (bool.TryParse(EmitNuspec, out var emitSpec) && emitSpec && !string.IsNullOrEmpty(NuspecFile))
                    GenerateNuspec();

                OutputPackage = new TaskItem(TargetPath);
                Manifest.CopyMetadataTo(OutputPackage);
                if (emitSpec)
                    OutputPackage.SetMetadata("Nuspec", NuspecFile);

                return !Log.HasLoggedErrors;
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
                return false;
            }
        }

        public Manifest Execute(Stream output) => Execute(output, out _);

        // Implementation for testing to avoid I/O
        public Manifest Execute(Stream output, out Manifest manifest)
        {
            GeneratePackage(output);
            manifest = this.manifest;

            output.Seek(0, SeekOrigin.Begin);
            using var reader = new PackageArchiveReader(output);

            return reader.GetManifest();
        }

        public Manifest CreateManifest()
        {
            var metadata = new ManifestMetadata
            {
                Id = Manifest.GetNullableMetadata(MetadataName.PackageId) ?? Manifest.GetMetadata("Id")
            };

            if (Manifest.TryGetMetadata(MetadataName.Version, out var version))
                metadata.Version = NuGetVersion.Parse(version);

            if (Manifest.TryGetBoolMetadata(nameof(ManifestMetadata.DevelopmentDependency), out var devDep) && devDep)
                metadata.DevelopmentDependency = true;

            if (Manifest.TryGetMetadata("Title", out var title))
                metadata.Title = ReplaceTokens(title.TrimIndent());

            if (Manifest.TryGetMetadata("Description", out var description))
                metadata.Description = ReplaceTokens(description.TrimIndent());

            if (Manifest.TryGetMetadata("Summary", out var summary))
                metadata.Summary = ReplaceTokens(summary.TrimIndent());

            if (Manifest.TryGetMetadata("Readme", out var readme))
                metadata.Readme = readme;

            if (Manifest.TryGetMetadata("Language", out var language))
                metadata.Language = language;

            if (Manifest.TryGetMetadata("Copyright", out var copyright))
                metadata.Copyright = ReplaceTokens(copyright);

            if (Manifest.TryGetBoolMetadata("RequireLicenseAcceptance", out var requireLicenseAcceptance) &&
                requireLicenseAcceptance)
                metadata.RequireLicenseAcceptance = requireLicenseAcceptance;

            if (!string.IsNullOrEmpty(Manifest.GetMetadata("Authors")))
                metadata.Authors = Manifest.GetMetadata("Authors").Split([','], StringSplitOptions.RemoveEmptyEntries);

            if (!string.IsNullOrEmpty(Manifest.GetMetadata("Owners")))
                metadata.Owners = Manifest.GetMetadata("Owners").Split([','], StringSplitOptions.RemoveEmptyEntries);

            if (!string.IsNullOrEmpty(Manifest.GetMetadata("LicenseUrl")))
                metadata.SetLicenseUrl(Manifest.GetMetadata("LicenseUrl"));

            if (Manifest.TryGetMetadata("LicenseExpression", out var expression))
            {
                metadata.LicenseMetadata = new LicenseMetadata(
                    LicenseType.Expression,
                    expression,
                    NuGetLicenseExpression.Parse(expression),
                    null, LicenseMetadata.CurrentVersion);
            }

            if (Manifest.TryGetMetadata("LicenseFile", out var file))
            {
                metadata.LicenseMetadata = new LicenseMetadata(
                    LicenseType.File,
                    file,
                    null,
                    null, LicenseMetadata.CurrentVersion);
            }

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

            if (Manifest.TryGetMetadata("RepositorySha", out var repoSha))
                (metadata.Repository ??= new RepositoryMetadata()).Commit = repoSha;

            if (!string.IsNullOrEmpty(Manifest.GetMetadata("ProjectUrl")))
                metadata.SetProjectUrl(Manifest.GetMetadata("ProjectUrl"));

            if (!string.IsNullOrEmpty(Manifest.GetMetadata("ProjectUrl")))
                metadata.SetProjectUrl(Manifest.GetMetadata("ProjectUrl"));

            if (!string.IsNullOrEmpty(Manifest.GetMetadata("IconUrl")))
                metadata.SetIconUrl(Manifest.GetMetadata("IconUrl"));

            if (Manifest.TryGetMetadata("Icon", out var icon))
                metadata.Icon = icon;

            if (Manifest.TryGetMetadata("ReleaseNotes", out var releaseNotes))
                metadata.ReleaseNotes = ReplaceTokens(releaseNotes.TrimIndent());

            if (Manifest.TryGetMetadata("Tags", out var tags))
                metadata.Tags = tags;

            if (Manifest.TryGetMetadata("MinClientVersion", out var minClientVersion))
                metadata.MinClientVersionString = minClientVersion;

            metadata.PackageTypes = ParsePackageTypes(Manifest.GetMetadata("PackageTypes"));

            var manifest = new Manifest(metadata);

            AddDependencies(manifest);
            AddFiles(manifest);
            AddFrameworkAssemblies(manifest);

            return manifest;
        }

        string? ReplaceTokens(string? text)
        {
            tokens ??= ReplacementTokens
                .Select(x => (x.ItemSpec.ToLowerInvariant(), x.GetMetadata("Value")))
                .GroupBy(t => t.Item1)
                .ToDictionary(g => g.Key, g => g.Last().Item2);

            tokensExpr ??= new Regex(@"\$(" + string.Join("|", tokens.Keys.Select(Regex.Escape)) + @")\$", RegexOptions.IgnoreCase);

            if (string.IsNullOrEmpty(text) || tokens.Count == 0)
                return text;

            return tokensExpr.Replace(text, match => tokens[match.Groups[1].Value.ToLower()]);
        }

        void GeneratePackage(Stream output = null)
        {
            manifest ??= CreateManifest();

            var builder = new PackageBuilder();
            builder.Populate(manifest.Metadata);

            // We don't use PopulateFiles because that performs search expansion, base path 
            // extraction and the like, which messes with our determined files to include.

            if (!string.IsNullOrEmpty(manifest.Metadata.Readme) &&
                manifest.Files.FirstOrDefault(f => Path.GetFileName(f.Target) == manifest.Metadata.Readme) is ManifestFile readmeFile &&
                File.Exists(readmeFile.Source))
            {
                // replace readme with includes replaced.
                var replaced = ReplaceTokens(IncludesResolver.Process(readmeFile.Source, message => Log.LogWarningCode("NG001", message)));

                if (manifest.Metadata.Repository?.Type == "git" &&
                    !string.IsNullOrEmpty(manifest.Metadata.Repository?.Commit) &&
                    Uri.TryCreate(manifest.Metadata.Repository.Url, UriKind.Absolute, out var uri) &&
                    uri.Host.EndsWith("github.com"))
                {
                    // expr to match markdown links with optional title. use named groups to capture the link text, url and optional title.
                    // Handle image links inside clickable badges: [![alt](img)](url) by explicitly matching the image pattern
                    linkExpr ??= new Regex(@"\[(?<text>!\[[^\]]*\]\([^\)]*\)|[^\]]+)\]\((?<url>[^\s)]+)(?:\s+""(?<title>[^""]*)"")?\)", RegexOptions.None);
                    var repoUrl = manifest.Metadata.Repository.Url.TrimEnd('/');

                    // Extract owner and repo from URL for raw.githubusercontent.com format
                    var repoPath = uri.AbsolutePath.TrimStart('/');
                    var rawBaseUrl = $"https://raw.githubusercontent.com/{repoPath}";

                    replaced = linkExpr.Replace(replaced, match =>
                    {
                        var url = match.Groups["url"].Value;
                        var title = match.Groups["title"].Value;

                        // Check if the URL is already absolute
                        if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
                            return match.Value;

                        // Use raw.githubusercontent.com format for proper image display on nuget.org
                        var newUrl = $"{rawBaseUrl}/{manifest.Metadata.Repository.Commit}/{url.TrimStart('/')}";

                        // Preserve the title if present
                        if (!string.IsNullOrEmpty(title))
                            return $"[{match.Groups["text"].Value}]({newUrl} \"{title}\")";

                        return $"[{match.Groups["text"].Value}]({newUrl})";
                    });
                }

                if (!replaced.Equals(File.ReadAllText(readmeFile.Source), StringComparison.Ordinal))
                {
                    var temp = Path.GetTempFileName();
                    File.WriteAllText(temp, replaced);
                    readmeFile.Source = temp;
                }
            }

            if (manifest.Metadata.LicenseMetadata?.Type == LicenseType.File &&
                manifest.Files.FirstOrDefault(f => Path.GetFileName(f.Target) == manifest.Metadata.LicenseMetadata.License) is ManifestFile licenseFile &&
                File.Exists(licenseFile.Source))
            {
                // replace readme with includes replaced.
                var replaced = ReplaceTokens(IncludesResolver.Process(licenseFile.Source, message => Log.LogWarningCode("NG001", message)));
                if (!replaced.Equals(File.ReadAllText(licenseFile.Source), StringComparison.Ordinal))
                {
                    var temp = Path.GetTempFileName();
                    File.WriteAllText(temp, replaced);
                    licenseFile.Source = temp;
                }
            }

            builder.Files.AddRange(manifest.Files.Select(file =>
                new PhysicalPackageFile { SourcePath = file.Source, TargetPath = file.Target }));

            if (output == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(TargetPath));
                using var stream = File.Create(TargetPath);
                builder.Save(stream);
            }
            else
            {
                builder.Save(output);
            }
        }

        void GenerateNuspec()
        {
            manifest ??= CreateManifest();

            Directory.CreateDirectory(Path.GetDirectoryName(NuspecFile));

            using var stream = File.Create(NuspecFile);
            manifest.Save(stream, true);
        }

        void AddDependencies(Manifest manifest)
        {
            var dependencies = from item in Contents
                               where PackFolderKind.Dependency.Equals(item.GetMetadata(MetadataName.PackFolder), StringComparison.OrdinalIgnoreCase) &&
                                     (!"all".Equals(item.GetMetadata(MetadataName.PrivateAssets), StringComparison.OrdinalIgnoreCase) ||
                                     "true".Equals(item.GetMetadata("Pack"), StringComparison.OrdinalIgnoreCase))
                               select new Dependency
                               {
                                   Id = item.ItemSpec,
                                   Version = VersionRange.Parse(item.GetMetadata(MetadataName.Version)),
                                   // PackFolderKind=Dependency has FrameworkSpecific=true, so pass that along
                                   TargetFramework = item.GetNuGetTargetFramework(true) ?? NuGetFramework.AnyFramework,
                                   Include = item.GetNullableMetadata(MetadataName.PackInclude) ?? item.GetNullableMetadata(MetadataName.IncludeAssets),
                                   Exclude = item.GetNullableMetadata(MetadataName.PackExclude) ?? item.GetNullableMetadata(MetadataName.ExcludeAssets)
                               };

            var definedDependencyGroups = (from dependency in dependencies
                                           group dependency by dependency.TargetFramework into dependenciesByFramework
                                           let targetFramework = dependenciesByFramework.Key
                                           let packages = (from dependency in dependenciesByFramework
                                                           where dependency.Id != "_._"
                                                           group dependency by dependency.Id into dependenciesById
                                                           select new PackageDependency
                                                           (
                                                                dependenciesById.Key,
                                                                dependenciesById.Select(x => x.Version).Aggregate(AggregateVersions),
                                                                dependenciesById.Select(x => x.Include).Aggregate(default(List<string>), AggregateAssetsFlow),
                                                                dependenciesById.Select(x => x.Exclude).Aggregate(default(List<string>), AggregateAssetsFlow)
                                                           )).ToList()
                                           select new PackageDependencyGroup(targetFramework, packages)
                                           //).ToList();
                                           ).ToDictionary(p => p.TargetFramework.GetFrameworkString());

            var libframeworks = (from item in Contents
                                 where PackFolderKind.Lib.Equals(item.GetMetadata(MetadataName.PackFolder), StringComparison.OrdinalIgnoreCase) &&
                                       !"all".Equals(item.GetMetadata(MetadataName.PrivateAssets), StringComparison.OrdinalIgnoreCase)
                                 // PackFolderKind=Lib has FrameworkSpecific=true, so pass that along
                                 select item.GetNuGetTargetFramework(true) ?? NuGetFramework.AnyFramework
                                 ).ToList();

            // include frameworks referenced by libraries, but without dependencies..
            foreach (var targetFramework in libframeworks
                .Where(f => !definedDependencyGroups.ContainsKey(f.GetFrameworkString())))
            {
                definedDependencyGroups.Add(targetFramework.GetFrameworkString(),
                    new PackageDependencyGroup(targetFramework, Array.Empty<PackageDependency>()));
            }

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
                    // Support nugetize CLI by ignoring missing files. When creating the final .nupkg, 
                    // file existence is checked always already
                    File.Exists(item.GetMetadata("FullPath")) ? File.GetLastWriteTime(item.GetMetadata("FullPath")) : DateTime.Now,
                    File.Exists(item.GetMetadata("FullPath")) ? new FileInfo(item.GetMetadata("FullPath")).Length : 0))
                .ToDictionary(item => item.Key, item => item.ToArray());

            // Add the ones we already determined to be duplicates that can safely be 
            // unified by package path, file name, last write time and file length.
            contents.AddRange(groupedByLastWriteAndLength
                .Where(group => group.Value.Length > 1)
                .Select(group => group.Value.First()));

            var md5 = new Lazy<HashAlgorithm>(() => MD5.Create());
            string hash(ITaskItem item)
            {
                // Support nugetize CLI by ignoring missing files. When creating the final .nupkg, 
                // file existence is checked always already
                if (!File.Exists(item.GetMetadata("FullPath")))
                    return item.GetMetadata("FullPath");

                using var file = File.OpenRead(item.GetMetadata("FullPath"));
                return string.Concat(md5.Value.ComputeHash(file).Select(x => x.ToString("x2")));
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
                                       where item.GetMetadata(MetadataName.PackFolder) == PackFolderKind.FrameworkReference
                                       select new FrameworkAssemblyReference
                                      (
                                          item.ItemSpec,
                                          // PackFolderKind=FrameworkReference has FrameworkSpecific=true, so pass that along
                                          new[] { item.GetNuGetTargetFramework(true) ?? NuGetFramework.AnyFramework }
                                      ))
                                      .ToList()
                                      .Distinct(FrameworkAssemblyReferenceComparer.Default)
                                      .ToList();

            manifest.Metadata.FrameworkReferences = frameworkReferences;
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

            aggregate ??= new List<string>(1);
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
                    var packageTypeSplitInPart = packageType.Split(new char[] { ',' });
                    var packageTypeName = packageTypeSplitInPart[0].Trim();
                    var version = PackageType.EmptyVersion;
                    if (packageTypeSplitInPart.Length > 1)
                    {
                        var versionString = packageTypeSplitInPart[1];
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
