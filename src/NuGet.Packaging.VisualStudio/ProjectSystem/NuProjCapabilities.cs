using System;
using Microsoft.VisualStudio.ProjectSystem;
using System.Collections.Immutable;
using Microsoft.VisualStudio.ProjectSystem.Utilities;

namespace NuGet.Packaging.VisualStudio
{
	internal static class NuProjCapabilities
	{
		/// <summary>
		/// A project capability that is present in your project type and none others.
		/// This is a convenient constant that may be used by your extensions so they
		/// only apply to instances of your project type.
		/// </summary>
		/// <remarks>
		/// This value should be kept in sync with the capability as actually defined in your .targets.
		/// </remarks>
		public const string NuProj = "NuGet.Packaging";

		public static readonly ImmutableHashSet<string> ProjectSystem = Empty.CapabilitiesSet.Union(new[]
		{
			NuProj,
			ProjectCapabilities.ProjectConfigurationsDeclaredAsItems,
			ProjectCapabilities.ReferencesFolder,
		});
	}
}
