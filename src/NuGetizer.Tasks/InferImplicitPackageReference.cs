using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
            var packages = new ConcurrentDictionary<PackageIdentity, List<PackageIdentity>>();
            Func<string, PackageIdentity> parse = value =>
            {
                var parts = value.Split('/');
                return new PackageIdentity(parts[0], parts[1]);
            };

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

            var inferred = new HashSet<PackageIdentity>();

            foreach (var reference in PackageReferences.Where(x =>
                "all".Equals(x.GetMetadata("PrivateAssets"), StringComparison.OrdinalIgnoreCase) &&
                // Unless explicitly set to Pack=false
                (!x.TryGetBoolMetadata("Pack", out var pack) || pack != false) &&
                // NETCore/NETStandard are implicitly defined, we never need to bring them as deps.
                !(bool.TryParse(x.GetMetadata("IsImplicitlyDefined"), out var isImplicit) && isImplicit)))
            {
                var identity = new PackageIdentity(reference.ItemSpec, reference.GetMetadata("Version"));
                foreach (var dependency in FindDependencies(identity, packages))
                {
                    inferred.Add(dependency);
                }
            }

            ImplicitPackageReferences = inferred
                .Select(x => new TaskItem(
                    x.Id,
                    new Dictionary<string, string>
                    {
                        { "Version", x.Version } ,
                        { "PrivateAssets", "all" },
                    }))
                .ToArray();

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
