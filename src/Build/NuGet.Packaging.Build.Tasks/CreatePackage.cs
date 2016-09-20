using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace NuGet.Packaging.Build.Tasks
{
	public class CreatePackage : Task
	{
		[Required]
		public string Id { get; set; }

		[Required]
		public string Version { get; set; }

		[Required]
		public ITaskItem[] Contents { get; set; }

		public string Authors { get; set; }

		public string Description { get; set; }

		public string Copyright { get; set; }

		public string RequireLicenseAcceptance { get; set; }

		public string LicenseUrl { get; set; }

		public string ProjectUrl { get; set; }

		public string IconUri { get; set; }

		public string ReleaseNotes { get; set; }

		public string Tags { get; set; }

		public string OutputPath { get; set; }

		public string Types { get; set; }

		public bool IsTool { get; set; }

		public string RepositoryUrl { get; set; }

		public string RepositoryType { get; set; }

		public bool NoPackageAnalysis { get; set; }

		public string MinClientVersion { get; set; }

		public ITaskItem[] AssemblyReferences { get; set; }

		public override bool Execute()
		{
			return true;
		}
	}
}
