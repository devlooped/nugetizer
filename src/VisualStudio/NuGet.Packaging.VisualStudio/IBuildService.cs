using Clide;
using Microsoft.Build.Execution;

namespace NuGet.Packaging.VisualStudio
{
	public interface IBuildService
	{
		void Pack(IProjectNode project);
	}
}