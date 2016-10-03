namespace NuGet.Packaging.VisualStudio
{
	using System;
	using System.Runtime.InteropServices;
	using Microsoft.VisualStudio.Shell;
	using System.ComponentModel.Design;
	using Microsoft.VisualStudio.ComponentModelHost;
	using EnvDTE;
	using Microsoft.VisualStudio.Shell.Interop;

	[Guid(Guids.PackageGuid)]
	[ProvideAutoLoad(UIContextGuids.SolutionExists)]
	// TODO: make sure we only auto-load when there are C# and VB projects. (what about F#?)
	//[ProvideAutoLoad("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}")]
	//[ProvideAutoLoad("{F184B08F-C81C-45F6-A57F-5ABD9991F28F}")]
	[PackageRegistration(UseManagedResourcesOnly = true)]
	[ProvideObject(typeof(NuSpecPropertyPage), RegisterUsing = RegistrationMethod.CodeBase)]
	[ProvideProjectFactory(
		typeof(NuProjFlavoredProjectFactory),
		"NuGet.Packaging",
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

			var extenders = this.GetService<ObjectExtenders>();
			if (extenders != null)
			{
				var provider = new NuGetExtenderProvider();
				foreach (var category in NuGetExtenderProvider.CategoryIds)
				{
					extenders.RegisterExtenderProvider(category, NuGetExtenderProvider.ExtenderName, provider);
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
