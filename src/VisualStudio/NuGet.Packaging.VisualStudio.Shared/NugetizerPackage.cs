namespace NuGet.Packaging.VisualStudio
{
	using System;
	using System.Runtime.InteropServices;
	using Microsoft.VisualStudio.Shell;
	using System.ComponentModel.Design;
	using Microsoft.VisualStudio.ComponentModelHost;
	using EnvDTE;
	using Microsoft.VisualStudio.Shell.Interop;
	using ExtenderProviders;
	using Microsoft.VisualStudio;

	[Guid(Guids.PackageGuid)]
	[PackageRegistration(UseManagedResourcesOnly = true)]
	[ProvideUIContextRule(
		Constants.UIContext.AddPlatformImplementation,
		name: "Portable Class Library UI Context",
		expression: "SolutionExistsAndNotBuildingAndNotDebugging & IsPortableClassLibrary",
		termNames: new[] { "SolutionExistsAndNotBuildingAndNotDebugging", "IsPortableClassLibrary" },
		termValues: new[] { VSConstants.UICONTEXT.SolutionExistsAndNotBuildingAndNotDebugging_string, "ActiveProjectFlavor:786C830F-07A1-408B-BD7F-6EE04809D6DB" })]
	[ProvideUIContextRule(
		Constants.UIContext.NonNuProj,
		name: "Non NuProj UI Context",
		expression: "SolutionExistsAndNotBuildingAndNotDebugging & !IsNuProj & (CSharpProjectContext | FSharpProjectContext | VBProjectContext | VCProjectContext)",
		termNames: new[] 
		{
			"SolutionExistsAndNotBuildingAndNotDebugging",
			"IsNuProj",
			"CSharpProjectContext",
			"VBProjectContext",
			"FSharpProjectContext",
			"VCProjectContext"
		},
		termValues: new[] 
		{
			VSConstants.UICONTEXT.SolutionExistsAndNotBuildingAndNotDebugging_string,
			"ActiveProjectCapability:PackagingProject",
			VSConstants.UICONTEXT.CSharpProject_string,
			VSConstants.UICONTEXT.VBProject_string,
			VSConstants.UICONTEXT.FSharpProject_string,
			VSConstants.UICONTEXT.VCProject_string,

		})]
	[ProvideMenuResource("2000", 2)]
	[ProvideBindingPath]
	public sealed class NuGetizerPackage : Package
	{
		IDisposable[] extenderProviders = new IDisposable[0];

		protected override void Initialize()
		{
			base.Initialize();

			//RegisterProjectFactory(new NuProjFlavoredProjectFactory(this));
			RegisterCommands();

			// These crash VS, investigating at vsixdisc DL
			//var extenders = this.GetService<ObjectExtenders>();
			//if (extenders != null)
			//{
			//	extenderProviders = new IDisposable[]
			//	{
			//		new LibraryProjectExtenderProvider(extenders),
			//		new NoneItemExtenderProvider(extenders),
			//		new ProjectReferenceExtenderProvider(extenders),
			//	};
			//}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				foreach (var disposable in extenderProviders)
				{
					disposable.Dispose();
				}
			}
		}

		void RegisterCommands()
		{
			var componentModel = this.GetService(typeof(SComponentModel)) as IComponentModel;
			var menuCommandService = this.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

			var commands = componentModel.DefaultExportProvider.GetExportedValues<DynamicCommand>();

			foreach (var command in commands)
				menuCommandService.AddCommand(command);
		}
	}
}
