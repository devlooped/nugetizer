using NuGet.Build.Packaging;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: Microsoft.VisualStudio.Shell.ProvideCodeBase(CodeBase = @"$PackageFolder$\NuGet.Packaging.VisualStudio.dll")]

[assembly: InternalsVisibleTo("NuGet.Packaging.VisualStudio.UnitTests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100653a25b8db304651a28d6a28454d201a4372d811143c969b92141cd87ffaa48d4e69f98d70d3ac689f69b673afcd44526a75903782c13573022fcd7fe05289af2abf5f7c195159650a974b8cc05eba30717bcfd882bf168690e4daaa4e1dd0aa393a420cf2d6ab2e89b286f81416f96737ae524039dbd0ff3d008e3dd8ec8de7")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2,PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]

[assembly: AssemblyCompany("Microsoft")]
[assembly: AssemblyProduct("Microsoft NuGet Packaging")]
[assembly: AssemblyCopyright("Copyright © 2016")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyTitle(ThisAssembly.Project.AssemblyName)]


#if DEBUG
[assembly: AssemblyConfiguration("DEBUG")]
#else
[assembly: AssemblyConfiguration ("RELEASE")]
#endif

[assembly: AssemblyVersion("1.0.0")]
[assembly: AssemblyFileVersion("1.0.0")]
[assembly: AssemblyInformationalVersion(
	ThisAssembly.Git.SemVer.Major + "." +
	ThisAssembly.Git.SemVer.Minor + "." +
	ThisAssembly.Git.SemVer.Patch + "-" +
	ThisAssembly.Git.Branch + "+" + ThisAssembly.Git.Commit)]