using Microsoft.VisualStudio.ProjectSystem;

namespace NuGet.Packaging.VisualStudio
{
    internal static class NuProjCapabilities
    {
		public const string NuProj = "PackagingProject";
		public const string OpenProjectFile = nameof(OpenProjectFile);

		public const string DefaultCapabilities = 
			ProjectCapabilities.HandlesOwnReload + "; " +
			OpenProjectFile + ";" +
			ProjectCapabilities.ProjectConfigurationsInferredFromUsage;
    }
}