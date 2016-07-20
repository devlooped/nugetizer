using System;
using System.Collections.Generic;
using NuGet;

namespace NuGet.Packaging.Tasks.Tests.Infrastructure
{
    public class PackageReferenceSetComparer : IEqualityComparer<PackageReferenceSet>
    {
        private static PackageReferenceSetComparer _instance = new PackageReferenceSetComparer(StringComparer.OrdinalIgnoreCase);

        private StringComparer _stringComparer;

        public PackageReferenceSetComparer(StringComparer stringComparer)
        {
            if (stringComparer == null)
            {
                throw new ArgumentNullException("stringComparer");
            }

            _stringComparer = stringComparer;
        }

        public static PackageReferenceSetComparer Instance
        {
            get
            {
                return _instance;
            }
        }

        public bool Equals(PackageReferenceSet x, PackageReferenceSet y)
        {
            if (x == null && x == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            var xReferences = new HashSet<string>(x.References.NullAsEmpty(), _stringComparer);
            var yReferences = new HashSet<string>(y.References.NullAsEmpty(), _stringComparer);

            return _stringComparer.Equals(x.TargetFramework, y.TargetFramework)
                && xReferences.SetEquals(yReferences);
        }

        public int GetHashCode(PackageReferenceSet obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            return _stringComparer.GetHashCode(obj.TargetFramework != null ? obj.TargetFramework.ToString() : "");
        }
    }
}