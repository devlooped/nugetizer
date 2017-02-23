using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using VSLangProj150;
using VSLangProj;
using Microsoft.VisualStudio.ProjectSystem.Properties;
using Microsoft.VisualStudio.ProjectSystem;
using Merq;
using System.Collections.Concurrent;

namespace NuGet.Packaging.VisualStudio
{
	class PackageReferencesService : PackageReferences
	{
		const string VersionMetadataName = "Version";

		readonly VSProject vsProject;
		readonly Lazy<IProjectLockService> projectLockService;
		readonly Lazy<IAsyncManager> asyncManager;

		public PackageReferencesService(
			VSProject vsProject,
			Lazy<IProjectLockService> projectLockService,
			Lazy<IAsyncManager> asyncManager)
		{
			this.vsProject = vsProject;
			this.projectLockService = projectLockService;
			this.asyncManager = asyncManager;
		}

		IProjectLockService ProjectLockService => projectLockService.Value;

		IAsyncManager AsyncManager => asyncManager.Value;

		public Project ContainingProject => vsProject.Project;

		public DTE DTE => vsProject.DTE;

		public Array InstalledPackages => GetPackageReferences().Select(x => x.EvaluatedInclude).ToArray();

		Microsoft.Build.Evaluation.ProjectItem GetPackageReference(string packageName) =>
			GetPackageReferences().FirstOrDefault(x => x.EvaluatedInclude == packageName);

		IEnumerable<Microsoft.Build.Evaluation.ProjectItem> GetPackageReferences() =>
			ExecuteLockAction(project => 
				project.GetItems(Constants.PackageReferenceItemName), 
				defaultValue: Enumerable.Empty<Microsoft.Build.Evaluation.ProjectItem>());

		void ExecuteLockAction(Action<Microsoft.Build.Evaluation.Project> callback, LockType lockType = LockType.Read) =>
			ExecuteLockAction(project => { callback(project); return true; }, lockType);

		TResult ExecuteLockAction<TResult>(Func<Microsoft.Build.Evaluation.Project, TResult> callback, LockType lockType = LockType.Read, TResult defaultValue = default(TResult))
		{
			var context = ContainingProject as IVsBrowseObjectContext;
			if (context == null)
				context = ContainingProject.Object as IVsBrowseObjectContext;

			if (context != null)
			{
				return AsyncManager.Run(async () =>
				{
					if (lockType == LockType.Read)
					{
						using (var access = await ProjectLockService.ReadLockAsync())
						{
							var configuredProject = await context.UnconfiguredProject.GetSuggestedConfiguredProjectAsync();

							var project = await access.GetProjectAsync(configuredProject);

							return callback(project);
						}
					}
					else
					{
						using (var access = await ProjectLockService.WriteLockAsync())
						{
							var configuredProject = await context.UnconfiguredProject.GetSuggestedConfiguredProjectAsync();

							var project = await access.GetProjectAsync(configuredProject);

							return callback(project);
						}
					}
				});
			}

			return defaultValue;
		}

		public dynamic Parent => vsProject;

		public void AddOrUpdate(string bstrName, string bstrVersion, Array pbstrMetadataElements, Array pbstrMetadataValues)
		{
			var item = GetPackageReference(bstrName);
			ExecuteLockAction(project =>
			{
				if (item == null)
				{
					item = project.AddItem(Constants.PackageReferenceItemName, bstrName).First();
				}

				item.SetMetadataValue(VersionMetadataName, bstrVersion);
				for (int i = 0; i < pbstrMetadataElements.Length; i++)
					item.SetMetadataValue((string)pbstrMetadataElements.GetValue(i), (string)pbstrMetadataValues.GetValue(i));

				// Hack/fix for: https://github.com/NuGet/Home/issues/4125#issuecomment-282023729
				if (bstrName == Constants.NuGet.BuildPackagingId)
					item.SetMetadataValue("PrivateAssets", "all");

			}, lockType: LockType.Write);
		}

		public void Remove(string bstrName)
		{
			var item = GetPackageReference(bstrName);
			if (item != null)
			{
				ExecuteLockAction(project =>
				{
					project.RemoveItem(item);
				}, lockType: LockType.Write);
			}
		}

		public bool TryGetReference(string bstrName, Array parrbstrDesiredMetadata, out string pbstrVersion, out Array pbstrMetadataElements, out Array pbstrMetadataValues)
		{
			var version = default(string);
			var metadata = new Dictionary<string, string>();

			var item = GetPackageReference(bstrName);
			if (item != null)
			{
				ExecuteLockAction(project =>
				{
					version = item.GetMetadataValue(VersionMetadataName);

					foreach (string metadataName in parrbstrDesiredMetadata)
						if (!metadata.ContainsKey(metadataName))
							metadata.Add(metadataName, item.GetMetadataValue(metadataName));
				});
			}

			pbstrVersion = version;
			pbstrMetadataElements = metadata.Any() ? metadata.Keys.ToArray() : null;
			pbstrMetadataValues = metadata.Any() ? metadata.Values.ToArray() : null;

			return item != null;
		}

		enum LockType
		{
			Read,
			Write
		}
	}
}
