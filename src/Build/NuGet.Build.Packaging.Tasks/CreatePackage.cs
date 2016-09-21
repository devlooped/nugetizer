using System;
using System.Linq;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Runtime.Versioning;
using NuGet.Versioning;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using NuGet.Packaging;

namespace NuGet.Build.Packaging.Tasks
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

		public string Owners { get; set; }

		public string Title { get; set; }

		public string Description { get; set; }

		public string Summary { get; set; }

		public string Language { get; set; }

		public string Copyright { get; set; }

		public string RequireLicenseAcceptance { get; set; }

		public string LicenseUrl { get; set; }

		public string ProjectUrl { get; set; }

		public string IconUrl { get; set; }

		public string ReleaseNotes { get; set; }

		public string Tags { get; set; }

		public string OutputPath { get; set; }

		public bool NoPackageAnalysis { get; set; }

		public string MinClientVersion { get; set; }

		public override bool Execute()
		{
			//  TODO: create file on OutputPath, invoke ExecuteImpl and return true.

			return true;
		}

		// Implementation for testing to avoid I/O
		public Manifest Execute(Stream output)
		{
			BuildPackage(output);

			output.Seek(0, SeekOrigin.Begin);
			var reader = new PackageArchiveReader(output);

			using (var stream = reader.GetNuspec())
			{
				return Manifest.ReadFrom(stream, true);
			}
		}

		public Manifest CreateManifest()
		{
			var metadata = new ManifestMetadata();

			metadata.Id = Id;
			if (!string.IsNullOrEmpty(Authors))
				metadata.Authors = Authors.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			if (!string.IsNullOrEmpty(Owners))
				metadata.Owners = Owners.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

			metadata.Title = Title;
			metadata.Description = Description;
			metadata.Summary = Summary;
			metadata.Language = Language;
			metadata.Copyright = Copyright;

			metadata.RequireLicenseAcceptance = string.IsNullOrEmpty(RequireLicenseAcceptance) ? false : bool.Parse(RequireLicenseAcceptance);
			metadata.SetLicenseUrl(LicenseUrl);
			metadata.SetProjectUrl(ProjectUrl);
			metadata.SetIconUrl(IconUrl);
			metadata.ReleaseNotes = ReleaseNotes;
			metadata.Tags = Tags;
			metadata.MinClientVersionString = MinClientVersion;
			metadata.Version = NuGetVersion.Parse(Version);

			var manifest = new Manifest(metadata);

			// TODO: Add files.
			AddDependencies(manifest);

			return manifest;
		}

		void AddDependencies(Manifest manifest)
		{
			// Collect all dependencies that are direct on the calling project, 
			// meaning they have a PackageId == Id being generated.
			var directDependencies = from item in Contents
									 where item.GetMetadata(MetadataName.Kind) == PackageItemKind.Dependency
									 let packageId = item.GetMetadata(MetadataName.PackageId)
									 where packageId == Id
									 select new Dependency
									 {
										 Id = item.ItemSpec,
										 Version = VersionRange.Parse(item.GetMetadata(MetadataName.Version)),
										 TargetFramework = item.GetTargetFramework()
									 };

			
			var dependencies = directDependencies; // + add Kind=Metadata as dependencies

			manifest.Metadata.DependencyGroups = (from dependency in dependencies
												  group dependency by dependency.TargetFramework into dependenciesByFramework
												  select new PackageDependencyGroup
												  (
													  NuGetFramework.Parse(dependenciesByFramework.Key.GetShortFrameworkName() ?? PackagingConstants.AnyFramework),
													  (from dependency in dependenciesByFramework
													   where dependency.Id != "_._"
													   group dependency by dependency.Id into dependenciesById
													   select new PackageDependency
													   (
														  dependenciesById.Key,
														  dependenciesById.Select(x => x.Version)
														  .Aggregate(AggregateVersions)
													   )).ToList()
												 )).ToList();
		}

		void BuildPackage(Stream output)
		{
			var builder = new PackageBuilder();
			var manifest = CreateManifest();

			builder.Populate(manifest.Metadata);
			// We shouldn't need the basePath at this point, since MSBuild will resolve everything to their full paths.
			builder.PopulateFiles("", manifest.Files);

			builder.Save(output);
		}

		static VersionRange AggregateVersions(VersionRange aggregate, VersionRange next)
		{
			var versionSpec = new VersionSpec();
			SetMinVersion(versionSpec, aggregate);
			SetMinVersion(versionSpec, next);
			SetMaxVersion(versionSpec, aggregate);
			SetMaxVersion(versionSpec, next);

			if (versionSpec.MinVersion == null && versionSpec.MaxVersion == null)
				return null;

			return versionSpec.ToVersionRange();
		}

		static void SetMinVersion(VersionSpec target, VersionRange source)
		{
			if (source == null || source.MinVersion == null)
				return;

			if (target.MinVersion == null)
			{
				target.MinVersion = source.MinVersion;
				target.IsMinInclusive = source.IsMinInclusive;
			}

			if (target.MinVersion < source.MinVersion)
			{
				target.MinVersion = source.MinVersion;
				target.IsMinInclusive = source.IsMinInclusive;
			}

			if (target.MinVersion == source.MinVersion)
				target.IsMinInclusive = target.IsMinInclusive && source.IsMinInclusive;
		}

		static void SetMaxVersion(VersionSpec target, VersionRange source)
		{
			if (source == null || source.MaxVersion == null)
				return;

			if (target.MaxVersion == null)
			{
				target.MaxVersion = source.MaxVersion;
				target.IsMaxInclusive = source.IsMaxInclusive;
			}

			if (target.MaxVersion > source.MaxVersion)
			{
				target.MaxVersion = source.MaxVersion;
				target.IsMaxInclusive = source.IsMaxInclusive;
			}

			if (target.MaxVersion == source.MaxVersion)
				target.IsMaxInclusive = target.IsMaxInclusive && source.IsMaxInclusive;
		}


		class Dependency
		{
			public string Id { get; set; }

			public FrameworkName TargetFramework { get; set; }

			public VersionRange Version { get; set; }
		}

		class VersionSpec
		{
			public bool IsMinInclusive { get; set; }
			public NuGetVersion MinVersion { get; set; }
			public bool IsMaxInclusive { get; set; }
			public NuGetVersion MaxVersion { get; set; }

			public VersionRange ToVersionRange()
			{
				return new VersionRange(MinVersion, IsMinInclusive, MaxVersion, IsMaxInclusive);
			}
		}
	}
}
