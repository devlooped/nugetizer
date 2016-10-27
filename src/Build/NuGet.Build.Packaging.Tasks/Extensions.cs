using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NuGet.Build.Packaging.Tasks;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;

namespace NuGet.Build.Packaging
{
	public static class Extensions
	{
		static readonly FrameworkName NullFramework = new FrameworkName("Null,Version=v1.0");

		public static IEnumerable<T> NullAsEmpty<T>(this IEnumerable<T> source) => source ?? Enumerable.Empty<T>();

		public static bool GetBoolean(this ITaskItem taskItem, string metadataName, bool defaultValue = false)
		{
			var result = false;
			var metadataValue = taskItem.GetMetadata(metadataName);

			return bool.TryParse(metadataValue, out result) && result;
		}

		public static string GetNullableMetadata(this ITaskItem taskItem, string metadataName)
		{
			var value = taskItem.GetMetadata(metadataName);
			if (string.IsNullOrEmpty(value))
				return null;

			return value;
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

		public static FrameworkName GetTargetFramework(this ITaskItem taskItem)
		{
			var metadataValue = taskItem.GetMetadata(MetadataName.TargetFramework);
			if (string.IsNullOrEmpty(metadataValue))
				metadataValue = taskItem.GetMetadata(MetadataName.TargetFrameworkMoniker);

			if (!string.IsNullOrEmpty(metadataValue))
				return new FrameworkName(NuGetFramework.Parse(metadataValue).DotNetFrameworkName);
			else
				return NullFramework;
		}

		public static FrameworkName GetTargetFrameworkMoniker(this ITaskItem item)
		{
			var value = item.GetMetadata(MetadataName.TargetFrameworkMoniker);
			// \o/: Turn .NETPortable,Version=v5.0 into .NETPlatform,Version=v5.0, hardcoded for now?
			// TODO: should be able to get .NETStandard,Version=v1.x from the item metadata somehow.

			return string.IsNullOrEmpty(value) ?
				NullFramework :
				new FrameworkName(value);
		}

		public static string GetShortFrameworkName(this FrameworkName frameworkName)
		{
			if (frameworkName == null || frameworkName == NullFramework)
				return null;

			// In this case, NuGet returns portable50, is that correct?
			//if (frameworkName.Identifier == ".NETPortable" && frameworkName.Version.Major == 5 && frameworkName.Version.Minor == 0)
			//	return "dotnet";

			return NuGetFramework.Parse(frameworkName.FullName).GetShortFolderName();
		}

		public static void LogErrorCode(this TaskLoggingHelper log, string code, string message, params object[] messageArgs) =>
			log.LogError(string.Empty, code, string.Empty, string.Empty, 0, 0, 0, 0, message, messageArgs);

		public static void LogWarningCode(this TaskLoggingHelper log, string code, string file, string message, params object[] messageArgs) =>
			log.LogWarning(string.Empty, code, string.Empty, file, 0, 0, 0, 0, message, messageArgs);
	}
}
