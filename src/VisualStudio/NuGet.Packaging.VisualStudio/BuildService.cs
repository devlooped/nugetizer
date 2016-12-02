using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ComponentModel.Composition;
using Clide;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;

namespace NuGet.Packaging.VisualStudio
{
	[Export(typeof(IBuildService))]
	[PartCreationPolicy(CreationPolicy.Shared)]
	public class BuildService : IBuildService, IVsUpdateSolutionEvents
	{
		const string PackOnBuildFilename = ".packonbuild";

		readonly uint updateSolutionEventsCookie;
		readonly IVsSolutionBuildManager2 buildManager;

		List<string> cleanupFilesOnBuildDone = new List<string>();

		[ImportingConstructor]
		public BuildService([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
		{
			buildManager = serviceProvider.GetService<SVsSolutionBuildManager, IVsSolutionBuildManager2>();
			buildManager.AdviseUpdateSolutionEvents(this, out updateSolutionEventsCookie);
		}

		public void Pack(IProjectNode project)
		{
			var hierarchy = project.AsVsHierarchy();
			if (hierarchy != null)
			{
				var targetPackOnBuildFile = Path.Combine(
					Path.GetDirectoryName(project.PhysicalPath), PackOnBuildFilename);

				// Write the .packonbuild empty file which is used by the targets
				// to override the PackOnBuild property and generate the .nupkg
				if (!File.Exists(targetPackOnBuildFile))
					File.WriteAllText(targetPackOnBuildFile, string.Empty);

				cleanupFilesOnBuildDone.Add(targetPackOnBuildFile);

				buildManager.StartSimpleUpdateProjectConfiguration(hierarchy, null, null,
					(uint)(VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_FORCE_UPDATE | VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_BUILD), 0, 0);
			}
		}

		int IVsUpdateSolutionEvents.OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy) => VSConstants.S_OK;

		int IVsUpdateSolutionEvents.UpdateSolution_Begin(ref int pfCancelUpdate) => VSConstants.S_OK;

		int IVsUpdateSolutionEvents.UpdateSolution_Cancel() => VSConstants.S_OK;

		int IVsUpdateSolutionEvents.UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
		{
			// Once the build is done, delete the .packonbuild files
			foreach (var file in cleanupFilesOnBuildDone)
				if (File.Exists(file))
					File.Delete(file);

			cleanupFilesOnBuildDone.Clear();

			return VSConstants.S_OK;
		}

		int IVsUpdateSolutionEvents.UpdateSolution_StartUpdate(ref int pfCancelUpdate) => VSConstants.S_OK;
	}
}