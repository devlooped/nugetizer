using System;
using System.Collections.Generic;
using NuGet.Packaging;
using NuGet.Packaging.Core;

namespace NuGet.Build.Packaging
{
	public class ManifestFileComparer : IEqualityComparer<ManifestFile>
    {
		public static ManifestFileComparer Default { get; } = new ManifestFileComparer();

        public bool Equals(ManifestFile x, ManifestFile y)
        {
            if (x == null && x == null)
                return true;
            if (x == null || y == null)
                return false;

			// We don't compare the Source since it's different for packaged manifests 
			return StringComparer.OrdinalIgnoreCase.Equals(x.Target, y.Target)
				&& StringComparer.OrdinalIgnoreCase.Equals(x.Exclude, x.Exclude);
        }

        public int GetHashCode(ManifestFile obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

			return StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Target ?? "")
				+ StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Exclude ?? "");
		}
	}
}
