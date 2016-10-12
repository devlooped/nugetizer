using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using System.ComponentModel.Composition;
using Clide;

namespace NuGet.Packaging.VisualStudio
{
	[Export(typeof(IMsBuildService))]
	[PartCreationPolicy(CreationPolicy.Shared)]
	public class MsBuildService : IMsBuildService
	{
		public BuildSubmission BeginBuild(string projectPath, string target = "Build")
		{
			var msbuildProject = ProjectCollection
				.GlobalProjectCollection
				.GetLoadedProjects(projectPath)
				.FirstOrDefault();

			if (msbuildProject != null)
			{
				var instance = msbuildProject.CreateProjectInstance();
				var parameters = new BuildParameters(ProjectCollection.GlobalProjectCollection);
				var requestData = new BuildRequestData(instance, new[] { target });

				BuildManager.DefaultBuildManager.BeginBuild(parameters);

				return BuildManager.DefaultBuildManager.PendBuildRequest(requestData);
			}

			return null;
		}
	}
}