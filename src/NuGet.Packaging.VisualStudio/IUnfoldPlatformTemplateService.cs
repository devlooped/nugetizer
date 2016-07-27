using Clide;

namespace NuGet.Packaging.VisualStudio
{
	interface IUnfoldPlatformTemplateService
	{
		IProjectNode UnfoldTemplate(string platformId, string path);
	}
}
