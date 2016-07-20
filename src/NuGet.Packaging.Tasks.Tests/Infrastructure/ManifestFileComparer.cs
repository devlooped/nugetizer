using System;
using System.Collections.Generic;

using NuGet;

namespace NuGet.Packaging.Tasks.Tests.Infrastructure
{
    public class ManifestFileComparer : IEqualityComparer<ManifestFile>
    {
        private static ManifestFileComparer _instance = new ManifestFileComparer(StringComparer.OrdinalIgnoreCase);

        private StringComparer _stringComparer;

        public ManifestFileComparer(StringComparer stringComparer)
        {
            if (stringComparer == null)
            {
                throw new ArgumentNullException("stringComparer");
            }

            _stringComparer = stringComparer;
        }

        public static ManifestFileComparer Instance
        {
            get
            {
                return _instance;
            }
        }

        public bool Equals(ManifestFile x, ManifestFile y)
        {
            if (x == null && x == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            return _stringComparer.Equals(x.Source, y.Source)
                && _stringComparer.Equals(x.Target, y.Target)
                && _stringComparer.Equals(x.Exclude, x.Exclude);
        }

        public int GetHashCode(ManifestFile obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            return _stringComparer.GetHashCode(obj.Source ?? "")
                + _stringComparer.GetHashCode(obj.Target ?? "")
                + _stringComparer.GetHashCode(obj.Exclude ?? "");
        }
    }
}