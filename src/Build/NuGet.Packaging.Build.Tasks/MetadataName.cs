namespace NuGet.Packaging.Build.Tasks
{
	public static class MetadataName
	{
		public const string FileSource = "FullPath";

		public const string Kind = nameof(Kind);

		/// <summary>
		/// One of https://github.com/NuGet/NuGet.Client/blob/dev/src/NuGet.Core/NuGet.Packaging/PackagingConstants.cs#L27
		/// </summary>
		public const string PackageFolder = nameof(PackageFolder);

		/// <summary>
		/// Concatenation of <see cref="PackageFolder"/> and <see cref="TargetFramework"/>. 
		/// For <c>contentFiles</c>, also includes the <see cref="CodeLanguage"/> or <c>any</c> if 
		/// none was provided.
		/// </summary>
		public const string PackagePath = nameof(PackagePath);

		public const string TargetFramework = nameof(TargetFramework);

		public const string TargetFrameworkMoniker = nameof(TargetFrameworkMoniker);
	}
}