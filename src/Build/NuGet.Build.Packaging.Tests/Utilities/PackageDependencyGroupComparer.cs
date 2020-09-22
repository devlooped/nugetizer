using System;
using System.Collections.Generic;
using NuGet.Packaging;
using NuGet.Packaging.Core;

namespace NuGet.Build.Packaging
{
	class PackageDependencyGroupComparer : IEqualityComparer<PackageDependencyGroup>
    {
		public static PackageDependencyGroupComparer Default { get; } = new PackageDependencyGroupComparer();

        public bool Equals(PackageDependencyGroup x, PackageDependencyGroup y)
        {
            if (x == null && x == null)
                return true;
            if (x == null || y == null)
                return false;

            var xDependencies = new HashSet<PackageDependency>(x.Packages.NullAsEmpty(), PackageDependencyComparer.Default);
            var yDependencies = new HashSet<PackageDependency>(y.Packages.NullAsEmpty(), PackageDependencyComparer.Default);

            return StringComparer.OrdinalIgnoreCase.Equals(x.TargetFramework, y.TargetFramework)
                && xDependencies.SetEquals(yDependencies);
        }

        public int GetHashCode(PackageDependencyGroup obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            return StringComparer.OrdinalIgnoreCase.GetHashCode(obj.TargetFramework != null ? obj.TargetFramework.ToString() : "");
        }
    }
}