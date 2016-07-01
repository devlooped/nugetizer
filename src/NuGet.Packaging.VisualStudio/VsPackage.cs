namespace NuGet.Packaging.VisualStudio
{
	using System;
	using System.Runtime.InteropServices;
	using Microsoft.VisualStudio.Shell;

	[Guid(Guids.PackageGuid)]
	[PackageRegistration(UseManagedResourcesOnly = true)]
	public sealed class VsPackage : Package
	{
	}
}
