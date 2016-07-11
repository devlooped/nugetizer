namespace NuGet.Packaging.VisualStudio
{
	using System;
	using System.Runtime.InteropServices;
	using Microsoft.VisualStudio.Shell;

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
	public sealed class VsPackage : Package
	{
		protected override void Initialize()
		{
			base.Initialize();

			RegisterProjectFactory(new NuProjFlavoredProjectFactory(this));
		}
	}
}
