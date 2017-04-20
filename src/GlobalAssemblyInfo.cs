#pragma warning disable 0436

using System.Reflection;
using NuGet.Build.Packaging;

[assembly: AssemblyCompany ("Microsoft")]
[assembly: AssemblyProduct ("Microsoft NuGet Packaging")]
[assembly: AssemblyCopyright ("Copyright © 2016")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyTitle(ThisAssembly.Project.AssemblyName)]


#if DEBUG
[assembly: AssemblyConfiguration ("DEBUG")]
#else
[assembly: AssemblyConfiguration ("RELEASE")]
#endif

[assembly: AssemblyVersion(ThisAssembly.SimpleVersion)]
[assembly: AssemblyFileVersion(ThisAssembly.FullVersion)]
[assembly: AssemblyInformationalVersion(ThisAssembly.InformationalVersion)]

namespace NuGet.Build.Packaging
{
	partial class ThisAssembly
	{
		/// <summary>
		/// Simple release-like version number, with just major, minor and ending up in '0'.
		/// </summary>
		public const string SimpleVersion = Git.SemVer.Major + "." + Git.SemVer.Minor + ".0";

		/// <summary>
		/// Full version, including commits since base version file, like 4.0.598
		/// </summary>
		public const string FullVersion = SimpleVersion + "." + Git.SemVer.Patch;
		
		/// <summary>
		/// Full version, plus branch and commit short sha.
		/// </summary>
		public const string InformationalVersion = FullVersion + "-" + Git.Branch + "+" + Git.Commit;
    }
}

#pragma warning restore 0436