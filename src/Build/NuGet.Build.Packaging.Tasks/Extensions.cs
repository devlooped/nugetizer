using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NuGet.Frameworks;

namespace NuGet.Build.Packaging.Tasks
{
	public static class Extensions
	{
		static readonly FrameworkName NullFramework = new FrameworkName("Null,Version=v1.0");

		public static IEnumerable<T> NullAsEmpty<T>(this IEnumerable<T> source) => source ?? Enumerable.Empty<T>();

		public static FrameworkName GetTargetFramework(this ITaskItem taskItem)
		{
			var metadataValue = taskItem.GetMetadata(MetadataName.TargetFramework);
			if (!string.IsNullOrEmpty(metadataValue))
				return new FrameworkName(NuGetFramework.Parse(metadataValue).DotNetFrameworkName);
			else
				return NullFramework;
		}

		public static FrameworkName GetTargetFrameworkMoniker(this ITaskItem item)
		{
			var value = item.GetMetadata(MetadataName.TargetFrameworkMoniker);

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
	}
}
