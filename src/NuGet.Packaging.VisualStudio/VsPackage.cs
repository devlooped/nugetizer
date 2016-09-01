namespace NuGet.Packaging.VisualStudio
{
	using System;
	using System.Runtime.InteropServices;
	using Microsoft.VisualStudio.Shell;
	using System.ComponentModel.Design;
	using Microsoft.VisualStudio.ComponentModelHost;

	[Guid(Guids.PackageGuid)]
	[PackageRegistration(UseManagedResourcesOnly = true)]
	[ProvideObject(typeof(NuSpecPropertyPage), RegisterUsing = RegistrationMethod.CodeBase)]
	[ProvideProjectFactory(
		typeof(NuProjFlavoredProjectFactory),
		"NuProj.Packaging",
		"#1100",
		null,
		null
		, @"\..\NullPath",
		LanguageVsTemplate = "CSharp",
		ShowOnlySpecifiedTemplatesVsTemplate = true)]
	[ProvideMenuResource("2000", 1)]
	public sealed class VsPackage : Package
	{
		protected override void Initialize()
		{
			base.Initialize();

			RegisterProjectFactory(new NuProjFlavoredProjectFactory(this));
			RegisterCommands();
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
