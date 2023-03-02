using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.CSharp.RuntimeBinder;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGetizer.Tasks;

namespace NuGetizer
{
    public static class Extensions
    {
        static readonly FrameworkName NullFramework = new FrameworkName("Null,Version=v1.0");

        public static IEnumerable<T> NullAsEmpty<T>(this IEnumerable<T> source) => source ?? Enumerable.Empty<T>();

        /// <summary>
        /// Tries to get a non-empty metadata value.
        /// </summary>
        public static bool TryGetMetadata(this ITaskItem taskItem, string metadataName, [NotNullWhen(true)] out string? value)
        {
            value = taskItem.GetMetadata(metadataName);
            if (string.IsNullOrEmpty(value))
                return false;

            return true;
        }

        /// <summary>
        /// Tries to get a non-public property of the item
        /// </summary>
        public static bool TryDynamic(this ITaskItem taskItem, Func<dynamic, string> function, [NotNullWhen(true)] out string? value)
        {
            try
            {
                value = function(taskItem.AsDynamicReflection());
                if (string.IsNullOrEmpty(value))
                {
                    value = null;
                    return false;
                }

                return true;
            }
            catch (RuntimeBinderException)
            {
                value = null;
                return false;
            }
        }

        public static bool TryGetBoolMetadata(this ITaskItem taskItem, string metadataName, out bool value)
        {
            value = false;
            var metadataValue = taskItem.GetMetadata(metadataName);
            if (string.IsNullOrEmpty(metadataValue))
                return false;

            return bool.TryParse(metadataValue, out value);
        }

        public static bool GetBoolean(this ITaskItem taskItem, string metadataName, bool defaultValue = false)
        {
            var metadataValue = taskItem.GetMetadata(metadataName);

            return bool.TryParse(metadataValue, out var result) ? result : defaultValue;
        }

        public static string? GetNullableMetadata(this ITaskItem taskItem, string metadataName)
        {
            var value = taskItem.GetMetadata(metadataName);
            if (string.IsNullOrEmpty(value))
                return null;

            return value;
        }

        public static string GetContentFileInclude(this ITaskItem taskItem)
        {
            var include = taskItem.GetMetadata(MetadataName.PackagePath);
            if (include.StartsWith("contentFiles"))
                include = include.Substring(12);

            return include.TrimStart('/', '\\');
        }

        public static Manifest GetManifest(this IPackageCoreReader packageReader)
        {
            using (var stream = packageReader.GetNuspec())
            {
                var manifest = Manifest.ReadFrom(stream, true);
                manifest.Files.AddRange(packageReader.GetFiles()
                    // Skip the auto-added stuff
                    .Where(file =>
                        file != "[Content_Types].xml" &&
                        file != "_rels/.rels" &&
                        !file.EndsWith(".nuspec") &&
                        !file.EndsWith(".psmdcp"))
                    .Select(file => new ManifestFile
                    {
                        // Can't replicate the Source path as it was originally before adding 
                        // to the package, so leave it null to avoid false promises in tests.
                        //Source = file.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar),
                        Target = file.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
                    }));

                return manifest;
            }
        }

        public static NuGetFramework? GetNuGetTargetFramework(this ITaskItem taskItem, bool? frameworkSpecific = default)
        {
            if (bool.TryParse(taskItem.GetMetadata(MetadataName.FrameworkSpecific), out var fws))
                frameworkSpecific = fws;

            if (frameworkSpecific != true)
                return NuGetFramework.AnyFramework;

            var metadataValue = taskItem.GetMetadata(MetadataName.TargetFramework);
            if (string.IsNullOrEmpty(metadataValue))
                metadataValue = taskItem.GetMetadata(MetadataName.DefaultTargetFramework);

            if (!string.IsNullOrEmpty(metadataValue))
                return NuGetFramework.Parse(metadataValue);
            else
                return null;
        }

        public static void LogErrorCode(this TaskLoggingHelper log, string code, string message, params object[] messageArgs) =>
            log.LogError(string.Empty, code, string.Empty, string.Empty, 0, 0, 0, 0, message, messageArgs);

        public static void LogWarningCode(this TaskLoggingHelper log, string code, string message, params object[] messageArgs) =>
            log.LogWarning(string.Empty, code, string.Empty, string.Empty, 0, 0, 0, 0, message, messageArgs);
    }
}
