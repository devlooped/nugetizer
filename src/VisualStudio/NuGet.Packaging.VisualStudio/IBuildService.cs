using Clide;
using Microsoft.Build.Execution;

namespace NuGet.Packaging.VisualStudio
{
	public interface IBuildService
	{
		bool IsBusy { get; }
		void Pack(IProjectNode project);
	}
}