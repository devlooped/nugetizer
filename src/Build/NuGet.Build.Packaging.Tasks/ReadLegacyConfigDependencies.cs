using System;
using System.Linq;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Newtonsoft.Json.Linq;
using NuGet.ProjectManagement;
using System.Collections.Generic;
using NuGet.Packaging.Core;
using System.Xml.Linq;
using NuGet.Packaging;
using NuGet.Versioning;

namespace NuGet.Build.Packaging.Tasks
{
	public class ReadLegacyConfigDependencies : Task
	{
		[Required]
		public ITaskItem PackagesConfigPath { get; set; }

		[Output]
		public ITaskItem[] PackageReferences { get; set; }

		public override bool Execute()
		{
			var fullPath = PackagesConfigPath.GetMetadata("FullPath");
			var config = XDocument.Load(fullPath);
			PackageReferences = new PackagesConfigReader(config).GetPackages()
				.Where(reference => !reference.IsDevelopmentDependency)
				.Select(ConvertToTaskItem)
				.ToArray();

			return true;
		}

		static ITaskItem ConvertToTaskItem(PackageReference reference)
		{
			var metadata = new Dictionary<string, string>();
			if (reference.HasAllowedVersions)
				metadata.Add("Version", reference.AllowedVersions.ToString());
			else if (reference.PackageIdentity.HasVersion)
				metadata.Add("Version", VersionRange.Parse(reference.PackageIdentity.Version.ToString()).ToString());

			return new TaskItem(reference.PackageIdentity.Id, metadata);
		}
	}
}
