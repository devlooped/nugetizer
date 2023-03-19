using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace NuGetizer.Tasks
{
    public class InferImplicitPackageReference : Task
    {
        [Required]
        public ITaskItem[] PackageReferences { get; set; } = Array.Empty<ITaskItem>();

        [Required]
        public ITaskItem[] PackageDependencies { get; set; } = Array.Empty<ITaskItem>();

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
                        primaryReference.SetMetadata("Version", identity.Version);
                }
            }

            var inferred = new Dictionary<PackageIdentity, ITaskItem>();
            var direct = new HashSet<string>(PackageReferences.Select(x => x.ItemSpec));

            foreach (var reference in PackageReferences)
            {
                var identity = new PackageIdentity(reference.ItemSpec, reference.GetMetadata("Version"));
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
                    yield return dependency;
                    foreach (var child in FindDependencies(dependency, packages))
                    {
                        yield return child;
                    }
                }
            }
        }

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
