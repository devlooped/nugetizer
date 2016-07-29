using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Versioning;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NuGet.Versioning;

namespace NuGet.Packaging.Tasks
{
    public static class Extensions
    {
        private static readonly FrameworkName NullFramework = new FrameworkName("Null,Version=v1.0");

        public static bool GetBoolean(this ITaskItem taskItem, string metadataName, bool defaultValue = false)
        {
            bool result = false;
            var metadataValue = taskItem.GetMetadata(metadataName);
            bool.TryParse(metadataValue, out result);
            return result;
        }

        public static FrameworkName GetTargetFramework(this ITaskItem taskItem)
        {
            FrameworkName result = null;
            var metadataValue = taskItem.GetMetadata(Metadata.TargetFramework);
            if (!string.IsNullOrEmpty(metadataValue))
            {
                result = VersionUtility.ParseFrameworkName(metadataValue);
            }
            else
            {
                result = NullFramework;
            }

            return result;
        }

        public static FrameworkName GetTargetFrameworkMoniker(this ITaskItem taskItem)
        {
            FrameworkName result = null;
            var metadataValue = taskItem.GetMetadata(Metadata.TargetFrameworkMoniker);
            if (!string.IsNullOrEmpty(metadataValue))
            {
                result = new FrameworkName(metadataValue);
            }
            else
            {
                result = NullFramework;
            }

            return result;
        }

        public static PackageDirectory GetPackageDirectory(this ITaskItem taskItem)
        {
            var packageDirectoryName = taskItem.GetMetadata(Metadata.PackageDirectory);
            if (string.IsNullOrEmpty(packageDirectoryName))
            {
                return PackageDirectory.Lib;
            }

            PackageDirectory result;
            Enum.TryParse(packageDirectoryName, true, out result);
            return result;
        }

        public static VersionRange GetVersion(this ITaskItem taskItem)
        {
            VersionRange result = null;
            var metadataValue = taskItem.GetMetadata(Metadata.VersionConstraint);
            if (string.IsNullOrEmpty(metadataValue))
            {
                metadataValue = taskItem.GetMetadata(Metadata.Version);
            }

            if (!string.IsNullOrEmpty(metadataValue))
            {
                VersionRange.TryParse(metadataValue, out result);
            }

            return result;
        }

        public static IEnumerable<T> NullAsEmpty<T>(this IEnumerable<T> source)
        {
            if (source == null)
            {
                return Enumerable.Empty<T>();
            }

            return source;
        }

        public static string GetShortFrameworkName(this FrameworkName frameworkName)
        {
            if (frameworkName == null || frameworkName == NullFramework)
            {
                return null;
            }

            if (frameworkName.Identifier == ".NETPortable" && frameworkName.Version.Major == 5 && frameworkName.Version.Minor == 0)
            {
                // Avoid calling GetShortFrameworkName because NuGet throws ArgumentException
                // in this case.
                return "dotnet";
            }

            return VersionUtility.GetShortFrameworkName(frameworkName);
        }

        public static string ToStringSafe(this object value)
        {
            if (value == null)
            {
                return null;
            }

            return value.ToString();
        }

        public static void UpdateMember<T>(this T target, Expression<Func<T, string>> memberLamda, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            var memberSelectorExpression = memberLamda.Body as MemberExpression;
            if (memberSelectorExpression == null)
            {
                throw new InvalidOperationException("Invalid member expression.");
            }
            
            var property = memberSelectorExpression.Member as PropertyInfo;
            if (property == null)
            {
                throw new InvalidOperationException("Invalid member expression.");
            }
            
            property.SetValue(target, value, null);
        }

        public static void AddRangeToMember<T, TItem>(this T target, Expression<Func<T, IEnumerable<TItem>>> memberLamda, IEnumerable<TItem> value)
        {
            if (value == null || value.Count() == 0)
            {
                return;
            }
            
            var memberSelectorExpression = memberLamda.Body as MemberExpression;
            if (memberSelectorExpression == null)
            {
                throw new InvalidOperationException("Invalid member expression.");
            }

            var property = memberSelectorExpression.Member as PropertyInfo;
            if (property == null)
            {
                throw new InvalidOperationException("Invalid member expression.");
            }

            var list = (List<TItem>)property.GetValue(target) ?? new List<TItem>();
            list.AddRange(value);
            
            property.SetValue(target, list, null);
        }

        public static string Combine(this PackageDirectory packageDirectory, string targetFramework, string fileName)
        { 
            switch (packageDirectory)
            {
                case PackageDirectory.Root:
                    return fileName;
                case PackageDirectory.Content:
                    return Path.Combine(Constants.ContentDirectory, fileName);
                case PackageDirectory.Build:
                    return Path.Combine(Constants.BuildDirectory, fileName);
                case PackageDirectory.Lib:
                    return Path.Combine(Constants.LibDirectory, targetFramework, fileName);
                case PackageDirectory.Tools:
                    return Path.Combine(Constants.ToolsDirectory, fileName);
                default:
                    return fileName;
            }
        }

        public static ITaskItem CreatePackageReferenceTaskItem(this PackageReference packageReference, ITaskItem project)
        {
            var id = packageReference.PackageIdentity.Id;
            var version = packageReference.PackageIdentity.Version;
            var targetFramework = packageReference.TargetFramework;
            var isDevelopmentDependency = packageReference.IsDevelopmentDependency;
            var requireReinstallation = packageReference.RequireReinstallation;
            var versionConstraint = packageReference.AllowedVersions;

            var item = new TaskItem(id);
            project.CopyMetadataTo(item);

            item.SetMetadata("ProjectPath", project.GetMetadata("FullPath"));

            item.SetMetadata("IsDevelopmentDependency", isDevelopmentDependency.ToString());
            item.SetMetadata("RequireReinstallation", requireReinstallation.ToString());

            if (version != null)
                item.SetMetadata(Metadata.Version, version.ToString());

            if (targetFramework != null)
                item.SetMetadata(Metadata.TargetFramework, targetFramework.GetShortFolderName());

            if (versionConstraint != null)
                item.SetMetadata("VersionConstraint", versionConstraint.ToString());

            return item;
        }

        public static bool IsOlderVersionThan(this Core.PackageDependency package, Core.PackageDependency other)
        {
            return package.VersionRange.MinVersion < other.VersionRange.MinVersion;
        }
    }
}
