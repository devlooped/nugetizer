using System;
using Microsoft.VisualStudio.ProjectSystem;
using System.Collections.Immutable;
using Microsoft.VisualStudio.ProjectSystem.Utilities;

namespace NuGet.Packaging.VisualStudio
{
	internal static class NuProjCapabilities
	{
		public const string NuProj = "PackagingProject";

		public static readonly ImmutableHashSet<string> ProjectSystem = Empty.CapabilitiesSet.Union(new[]
		{
			NuProj,
			ProjectCapabilities.ProjectConfigurationsDeclaredAsItems,
			ProjectCapabilities.ReferencesFolder,
		});
	}
}
