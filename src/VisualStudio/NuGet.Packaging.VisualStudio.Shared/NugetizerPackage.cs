namespace NuGet.Packaging.VisualStudio
{
	using System;
	using System.Runtime.InteropServices;
	using Microsoft.VisualStudio.Shell;
	using System.ComponentModel.Design;
	using Microsoft.VisualStudio.ComponentModelHost;
	using Microsoft.VisualStudio;
	using Clide;
	using Microsoft.VisualStudio.Shell.Interop;

	[Guid(Guids.PackageGuid)]
	[PackageRegistration(UseManagedResourcesOnly = true)]
	[ProvideUIContextRule(
		Constants.UIContext.AddPlatformImplementation,
		name: "Portable Class Library UI Context",
		expression: "SolutionExistsAndNotBuildingAndNotDebugging & IsPortableClassLibrary",
		termNames: new[] { "SolutionExistsAndNotBuildingAndNotDebugging", "IsPortableClassLibrary" },
		termValues: new[] { VSConstants.UICONTEXT.SolutionExistsAndNotBuildingAndNotDebugging_string, "ActiveProjectFlavor:786C830F-07A1-408B-BD7F-6EE04809D6DB" })]
	[ProvideUIContextRule(
		Constants.UIContext.NuProj,
		name: "NuProj Exists",
		expression: "NuProj",
		termNames: new[] { "NuProj" },
		termValues: new[] { "SolutionHasProjectCapability:" + Constants.NuProjCapability })]
	[ProvideMenuResource("2000", 3)]
	[ProvideBindingPath]
	public sealed class NuGetizerPackage : Package
	{
		static readonly Guid NuGetPackageGuid = new Guid("5fcc8577-4feb-4d04-ad72-d6c629b083cc");

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

		public IServiceProvider GetLoadedPackage(Guid packageId)
		{
			try
			{
				var vsPackage = default(IVsPackage);

				var vsShell = GetService(typeof(SVsShell)) as IVsShell;
				vsShell.IsPackageLoaded(ref packageId, out vsPackage);

				if (vsPackage == null)
					ErrorHandler.ThrowOnFailure(vsShell.LoadPackage(ref packageId, out vsPackage));

				return (IServiceProvider)vsPackage;
			}
			catch { return null; }
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
