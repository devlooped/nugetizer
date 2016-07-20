using System;
using System.Collections.Generic;

namespace NuGet.Packaging.Tasks.Tests.Infrastructure
{
	public class PackageDependencyComparer : IEqualityComparer<Core.PackageDependency>
    {
        private StringComparer _stringComparer;
        
        public PackageDependencyComparer(StringComparer stringComparer)
        {
            if (stringComparer == null)
            {
                throw new ArgumentNullException("stringComparer");
            }

            _stringComparer = stringComparer;
        }
        
        public bool Equals(Core.PackageDependency x, Core.PackageDependency y)
        {
            if (x == null && x == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            return _stringComparer.Equals(x.Id, y.Id) && x.VersionRange.Equals(y.VersionRange);
        }

        public int GetHashCode(Core.PackageDependency obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            return _stringComparer.GetHashCode(obj.Id ?? "") + _stringComparer.GetHashCode(obj.VersionRange.ToString() ?? "");
        }
    }
}
