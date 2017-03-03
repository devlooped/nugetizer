using Microsoft.VisualStudio.ProjectSystem;

namespace NuGet.Packaging.VisualStudio
{
    internal static class NuProjCapabilities
    {
		public const string NuProj = "PackagingProject";

		public const string HandlesOwnReload = ProjectCapabilities.HandlesOwnReload;
		public const string OpenProjectFile = nameof(OpenProjectFile);

		public const string DefaultCapabilities = HandlesOwnReload + "; " +
												  OpenProjectFile;
    }
}