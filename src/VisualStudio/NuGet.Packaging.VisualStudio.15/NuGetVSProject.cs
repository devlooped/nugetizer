using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using VSLangProj;
using System.Runtime.InteropServices;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.ProjectSystem;
using VSLangProj150;
using VSLangProj140;
using VSLangProj80;
using Merq;

namespace NuGet.Packaging.VisualStudio
{
	[Export(Contracts.ProjectSystem_VSProject, typeof(VSProject))]
	[AppliesTo(Constants.NuProjCapability)]
	[ComVisible(true)]
	[Order(1)] // This Project needs to be exported before the built-in (OAVSProject) provided by VS
	class NuGetVSProject : VSProject, VSProject4
	{
		readonly Lazy<VSProject> innerProject;
		readonly Lazy<PackageReferences> packageReferences;
		readonly Lazy<References> references;
		readonly Lazy<VSProjectEvents2> events2;
		readonly Lazy<AnalyzerReferences> analyzerReferences;

		[ImportingConstructor]
		public NuGetVSProject(
			[ImportMany(Contracts.ProjectSystem_VSProject)] IEnumerable<Lazy<VSProject, IAppliesToMetadataView>> projects,
			Lazy<IProjectLockService> projectLockService,
			Lazy<IAsyncManager> asyncManager)
		{
			innerProject = new Lazy<VSProject>(() => projects
					.Where(x => x.Metadata.AppliesTo != Constants.NuProjCapability)
					.Select(x => x.Value)
					.FirstOrDefault());

			packageReferences = new Lazy<PackageReferences>(() => new PackageReferencesService(this, projectLockService, asyncManager));
			references = new Lazy<References>(() => new ReferencesWrapper(innerProject, packageReferences));
			events2 = new Lazy<VSProjectEvents2>(() => new ProjectEventsService(innerProject));
			analyzerReferences = new Lazy<AnalyzerReferences>(() => new AnalyzerReferencesService(innerProject));
		}

		VSProject InnerProject => innerProject.Value;

		public BuildManager BuildManager => InnerProject.BuildManager;

		public DTE DTE => InnerProject.DTE;

		public VSProjectEvents Events => InnerProject.Events;

		public Imports Imports => InnerProject.Imports;

		public Project Project => InnerProject.Project;

		public References References => references.Value;
		//public References References => InnerProject.References;

		public string TemplatePath => InnerProject.TemplatePath;

		public ProjectItem WebReferencesFolder => InnerProject.WebReferencesFolder;

		public bool WorkOffline
		{
			get { return InnerProject.WorkOffline; }
			set { InnerProject.WorkOffline = value; }
		}

		public PackageReferences PackageReferences => packageReferences.Value;

		public dynamic PublishManager => null;

		public VSProjectEvents2 Events2 => events2.Value;

		public AnalyzerReferences AnalyzerReferences => analyzerReferences.Value;

		public ProjectItem AddWebReference(string bstrUrl) => InnerProject.AddWebReference(bstrUrl);

		public void CopyProject(string bstrDestFolder, string bstrDestUNCPath, prjCopyProjectOption copyProjectOption, string bstrUsername, string bstrPassword) =>
			InnerProject.CopyProject(bstrDestFolder, bstrDestUNCPath, copyProjectOption, bstrUsername, bstrPassword);

		public ProjectItem CreateWebReferencesFolder() => InnerProject.CreateWebReferencesFolder();

		public void Exec(prjExecCommand command, int bSuppressUI, object varIn, out object pVarOut) =>
			InnerProject.Exec(command, bSuppressUI, varIn, out pVarOut);

		public void GenerateKeyPairFiles(string strPublicPrivateFile, string strPublicOnlyFile = "0") =>
			InnerProject.GenerateKeyPairFiles(strPublicPrivateFile, strPublicOnlyFile);

		public string GetUniqueFilename(object pDispatch, string bstrRoot, string bstrDesiredExt) =>
			InnerProject.GetUniqueFilename(pDispatch, bstrRoot, bstrDesiredExt);

		public void Refresh() => InnerProject.Refresh();
	}
}
