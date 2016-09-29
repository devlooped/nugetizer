using System;
using System.Collections.Generic;
using NuGet.Packaging.Core;

namespace NuGet.Build.Packaging
{
	public class PackageDependencyComparer : IEqualityComparer<PackageDependency>
    {
		public static PackageDependencyComparer Default { get; } = new PackageDependencyComparer();

        public bool Equals(PackageDependency x, PackageDependency y)
        {
            if (x == null && x == null)
                return true;
            if (x == null || y == null)
                return false;

            return StringComparer.OrdinalIgnoreCase.Equals(x.Id, y.Id) && x.VersionRange.Equals(y.VersionRange);
        }

        public int GetHashCode(PackageDependency obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            return StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Id ?? "") +
				StringComparer.OrdinalIgnoreCase.GetHashCode(obj.VersionRange.ToString() ?? "");
        }
    }
}
