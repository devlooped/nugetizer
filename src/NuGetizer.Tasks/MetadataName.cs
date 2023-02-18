namespace NuGetizer.Tasks
{
    public static class MetadataName
    {
        public const string FileSource = "FullPath";

        public const string PackFolder = nameof(PackFolder);

        public const string Version = nameof(Version);

        /// <summary>
        /// One of https://github.com/NuGet/NuGet.Client/blob/dev/src/NuGet.Core/NuGet.Packaging/PackagingConstants.cs#L27
        /// </summary>
        public const string PackageFolder = nameof(PackageFolder);

        /// <summary>
        /// The package that declares the given package file.
        /// </summary>
        public const string PackageId = nameof(PackageId);

        /// <summary>
        /// Concatenation of <see cref="PackageFolder"/> and <see cref="TargetFramework"/>. 
        /// For <c>contentFiles</c>, also includes the <see cref="CodeLanguage"/> or <c>any</c> if 
        /// none was provided.
        /// </summary>
        public const string PackagePath = nameof(PackagePath);

        /// <summary>
        /// Marks a @(PackageReference) as a development dependency when set to 'All'.
        /// </summary>
        public const string PrivateAssets = nameof(PrivateAssets);

        /// <summary>
        /// Assets to include for a dependency/package reference
        /// </summary>
        public const string IncludeAssets = nameof(IncludeAssets);

        /// <summary>
        /// Assets to exclude for a dependency/package reference
        /// </summary>
        public const string ExcludeAssets = nameof(ExcludeAssets);

        /// <summary>
        /// Same as <see cref="IncludeAssets"/>, but allows having a different value for the
        /// included assets in pack vs build/restore of the referencing project.
        /// </summary>
        public const string PackInclude = nameof(PackInclude);

        /// <summary>
        /// Same as <see cref="ExcludeAssets"/>, but allows having a different value for the
        /// excluded assets in pack vs build/restore of the referencing project.
        /// </summary>
        public const string PackExclude = nameof(PackExclude);

        /// <summary>
        /// Whether the project can be packed as a .nupkg.
        /// </summary>
        public const string IsPackable = nameof(IsPackable);

        /// <summary>
        /// Whether a PackageFile is framework-specific or not.
        /// </summary>
        public const string FrameworkSpecific = nameof(FrameworkSpecific);

        /// <summary>
        /// The target framework of an item.
        /// </summary>
        public const string TargetFramework = nameof(TargetFramework);

        /// <summary>
        /// The original (and therefore default) target framework of the project that declared an item.
        /// </summary>
        public const string DefaultTargetFramework = nameof(DefaultTargetFramework);

        /// <summary>
        /// Available optional metadata values of contentFiles.
        /// </summary>
        public static class ContentFile
        {
            public const string CodeLanguage = nameof(CodeLanguage);
            public const string BuildAction = nameof(BuildAction);
            public const string CopyToOutput = nameof(CopyToOutput);
            public const string Flatten = nameof(Flatten);
        }
    }
}