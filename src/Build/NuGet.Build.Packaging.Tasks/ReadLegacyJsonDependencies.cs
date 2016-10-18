using System;
using System.Linq;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Newtonsoft.Json.Linq;
using NuGet.ProjectManagement;
using System.Collections.Generic;
using NuGet.Packaging.Core;

namespace NuGet.Build.Packaging.Tasks
{
	public class ReadLegacyJsonDependencies : Task
	{
		[Required]
		public ITaskItem ProjectJsonPath { get; set; }

		[Output]
		public ITaskItem[] PackageReferences { get; set; }

		public override bool Execute()
		{
			var fullPath = ProjectJsonPath.GetMetadata("FullPath");
			using (var reader = new StreamReader(fullPath))
			{
				var json = JObject.Parse(reader.ReadToEnd());
				PackageReferences = JsonConfigUtility.GetDependencies(json)
					.Select(ConvertToTaskItem)
					.ToArray();
			}

			return true;
		}

		static ITaskItem ConvertToTaskItem(PackageDependency dependency)
		{
			var metadata = new Dictionary<string, string>
			{
				{ "Version", dependency.VersionRange.ToString() },
			};
			
			// NOTE: due to https://github.com/NuGet/Home/issues/520, 
			// we currently have no way of determining if the dependency is a 
			// development dependency. We'll just skip our own package id for now.
			if (dependency.Id == ThisAssembly.Project.Properties.PackageId)
				metadata.Add("PrivateAssets", "All");

			return new TaskItem(dependency.Id, metadata);
		}
	}
}
