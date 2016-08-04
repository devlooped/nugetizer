using System;
using System.Collections.Generic;

namespace NuGet.Packaging.Tasks.Tests.Infrastructure
{
    public class PackageDependencyGroupComparer : IEqualityComparer<PackageDependencyGroup>
    {
        private static PackageDependencyGroupComparer _instance = new PackageDependencyGroupComparer(StringComparer.OrdinalIgnoreCase);

		private PackageDependencyComparer _dependencyComparer;

        private StringComparer _stringComparer;

        public PackageDependencyGroupComparer(StringComparer stringComparer)
        {
            if (stringComparer == null)
            {
                throw new ArgumentNullException("stringComparer");
            }

            _stringComparer = stringComparer;
            _dependencyComparer = new PackageDependencyComparer(stringComparer);
        }

        public static PackageDependencyGroupComparer Instance
        {
            get
            {
                return _instance;
            }
        }

        public bool Equals(PackageDependencyGroup x, PackageDependencyGroup y)
        {
            if (x == null && x == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            var xDependencies = new HashSet<Core.PackageDependency>(x.Packages.NullAsEmpty(), _dependencyComparer);
            var yDependencies = new HashSet<Core.PackageDependency>(y.Packages.NullAsEmpty(), _dependencyComparer);

            return _stringComparer.Equals(x.TargetFramework, y.TargetFramework)
                && xDependencies.SetEquals(yDependencies);
        }

        public int GetHashCode(PackageDependencyGroup obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            return _stringComparer.GetHashCode(obj.TargetFramework != null ? obj.TargetFramework.ToString() : "");
        }
    }
}