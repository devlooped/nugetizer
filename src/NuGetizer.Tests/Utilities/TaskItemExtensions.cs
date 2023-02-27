using System;
using Microsoft.Build.Framework;
using NuGet.Packaging;

namespace NuGetizer
{
    static class TaskItemExtensions
    {
        /// <summary>
        /// Checks if the given item has metadata key/values matching the 
        /// anonymous object property/values.
        /// </summary>
        public static bool Matches(this ITaskItem item, object metadata)
        {
            foreach (var prop in metadata.GetType().GetProperties())
            {
                var actual = item.GetMetadata(prop.Name).Replace('\\', '/');
                var expected = prop.GetValue(metadata).ToString().Replace('\\', '/');

                if (!actual.Equals(expected, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return true;
        }

        public static Manifest GetManifest(this ITaskItem package)
        {
            using var reader = new PackageArchiveReader(package.GetMetadata("FullPath"));
            return reader.GetManifest();
        }
    }
}
