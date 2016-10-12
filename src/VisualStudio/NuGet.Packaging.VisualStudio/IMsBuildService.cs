using Clide;
using Microsoft.Build.Execution;

namespace NuGet.Packaging.VisualStudio
{
	public interface IMsBuildService
	{
		BuildSubmission BeginBuild(string projectPath, string target = "Build");
	}
}