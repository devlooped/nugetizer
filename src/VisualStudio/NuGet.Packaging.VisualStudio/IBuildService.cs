using Clide;

namespace NuGet.Packaging.VisualStudio
{
	public interface IBuildService
	{
		void Pack(IProjectNode project);
	}
}