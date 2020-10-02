using System;
using System.Collections.Generic;
using NuGet.Packaging;

namespace NuGetizer
{
    class ManifestContentFilesComparer : IEqualityComparer<ManifestContentFiles>
    {
        public static ManifestContentFilesComparer Default { get; } = new ManifestContentFilesComparer();

        public bool Equals(ManifestContentFiles x, ManifestContentFiles y)
        {
            if (x == null && x == null)
                return true;
            if (x == null || y == null)
                return false;

            return StringComparer.OrdinalIgnoreCase.Equals(x.Include, y.Include)
                && StringComparer.OrdinalIgnoreCase.Equals(x.BuildAction, y.BuildAction)
                && StringComparer.OrdinalIgnoreCase.Equals(x.CopyToOutput, x.CopyToOutput)
                && StringComparer.OrdinalIgnoreCase.Equals(x.Flatten, x.Flatten);
        }

        public int GetHashCode(ManifestContentFiles obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            return StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Include ?? "")
                + StringComparer.OrdinalIgnoreCase.GetHashCode(obj.BuildAction ?? "")
                + StringComparer.OrdinalIgnoreCase.GetHashCode(obj.CopyToOutput ?? "")
                + StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Flatten ?? "");
        }
    }
}
