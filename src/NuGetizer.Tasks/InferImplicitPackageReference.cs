using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NuGet.Packaging;

namespace NuGetizer.Tasks
{
    public class InferImplicitPackageReference : Task
    {
        [Required]
        public ITaskItem[] PackageReferences { get; set; } = Array.Empty<ITaskItem>();

        [Required]
        public ITaskItem[] PackageDependencies { get; set; } = Array.Empty<ITaskItem>();

        /// <summary>
        /// Optional package definitions (from the SDK's ResolvePackageDependencies), used 
        /// to locate each package's nuspec and skip development dependencies (and their 
        /// own dependencies) from transitive inference.
        /// </summary>
        public ITaskItem[] PackageDefinitions { get; set; } = Array.Empty<ITaskItem>();

        [Output]
        public ITaskItem[] ImplicitPackageReferences { get; set; } = Array.Empty<ITaskItem>();

        public override bool Execute()
        {
            if (Environment.GetEnvironmentVariable("DEBUG_NUGETIZER") == "1")
                Debugger.Launch();

            var packages = new ConcurrentDictionary<PackageIdentity, List<PackageIdentity>>();

            static PackageIdentity parse(string value)
            {
                var parts = value.Split('/');
                return new PackageIdentity(parts[0], parts[1]);
            }

            // Build the list of parent>child relationships.
            foreach (var dependency in PackageDependencies.Where(x => x.ItemSpec.Contains('/')))
            {
                var identity = parse(dependency.ItemSpec);
                var parent = dependency.GetMetadata("ParentPackage");
                if (!string.IsNullOrEmpty(parent))
                {
                    packages.GetOrAdd(parse(parent), _ => new List<PackageIdentity>())
                        .Add(identity);
                }
                else
                {
                    // In centrally managed package versions, at this point we have 
                    // the right version if the project is using centrally managed versions
                    var primaryReference = PackageReferences.FirstOrDefault(x => x.ItemSpec == identity.Id);
                    if (primaryReference != null && primaryReference.GetNullableMetadata("Version") == null)
                    {
                        // If VersionOverride is specified (CPM feature), use it instead of the resolved version
                        var versionOverride = primaryReference.GetNullableMetadata("VersionOverride");
                        primaryReference.SetMetadata("Version", versionOverride ?? identity.Version);
                    }
                }
            }

            foreach (var definition in PackageDefinitions.Where(x => x.ItemSpec.Contains('/')))
            {
                var path = definition.GetMetadata("ResolvedPath");
                if (!string.IsNullOrEmpty(path))
                    paths[parse(definition.ItemSpec)] = path;
            }

            var inferred = new Dictionary<PackageIdentity, ITaskItem>();
            var direct = new HashSet<string>(PackageReferences.Select(x => x.ItemSpec));

            foreach (var reference in PackageReferences)
            {
                var identity = new PackageIdentity(reference.ItemSpec, reference.GetMetadata("Version"));
                // Development dependencies are build-only, so their dependencies should never be packed.
                if (IsDevelopmentDependency(identity))
                    continue;

                var originalMetadata = (IDictionary<string, string>)reference.CloneCustomMetadata();
                foreach (var dependency in FindDependencies(identity, packages))
                {
                    if (!direct.Contains(dependency.Id) && !inferred.ContainsKey(dependency))
                    {
                        var item = new TaskItem(dependency.Id);
                        foreach (var metadata in originalMetadata)
                            item.SetMetadata(metadata.Key, metadata.Value);

                        item.SetMetadata("Version", dependency.Version);
                        inferred.Add(dependency, item);
                    }
                }
            }

            ImplicitPackageReferences = inferred.Values.ToArray();

            return true;
        }

        IEnumerable<PackageIdentity> FindDependencies(PackageIdentity identity, IDictionary<PackageIdentity, List<PackageIdentity>> packages)
        {
            if (packages.TryGetValue(identity, out var dependencies))
            {
                foreach (var dependency in dependencies)
                {
                    if (IsDevelopmentDependency(dependency))
                        continue;

                    yield return dependency;
                    foreach (var child in FindDependencies(dependency, packages))
                    {
                        yield return child;
                    }
                }
            }
        }

        bool IsDevelopmentDependency(PackageIdentity identity)
        {
            if (developmentDependencies.TryGetValue(identity, out var value))
                return value;

            value = false;
            if (paths.TryGetValue(identity, out var path) && Directory.Exists(path) &&
                Directory.EnumerateFiles(path, "*.nuspec").FirstOrDefault() is string nuspec)
            {
                try
                {
                    value = new NuspecReader(nuspec).GetDevelopmentDependency();
                }
                catch (Exception e)
                {
                    Log.LogMessage(MessageImportance.Low, $"Failed to read nuspec '{nuspec}': {e.Message}");
                }
            }

            developmentDependencies[identity] = value;
            return value;
        }

        readonly Dictionary<PackageIdentity, string> paths = new();
        readonly Dictionary<PackageIdentity, bool> developmentDependencies = new();

        class PackageIdentity
        {
            public PackageIdentity(string id, string version)
                => (Id, Version)
                = (id, version);

            public string Id { get; }
            public string Version { get; }

            public override bool Equals(object obj)
                => obj is PackageIdentity dependency &&
                    dependency.Id == Id &&
                    dependency.Version == Version;

            public override int GetHashCode() => Tuple.Create(Id, Version).GetHashCode();

            public override string ToString() => Id + "/" + Version;
        }
    }
}
