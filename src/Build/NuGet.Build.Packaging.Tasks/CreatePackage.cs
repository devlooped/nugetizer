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
using System.Collections.Generic;

namespace NuGet.Build.Packaging.Tasks
{
	public class CreatePackage : Task
	{
		[Required]
		public ITaskItem Manifest { get; set; }

		[Required]
		public ITaskItem[] Contents { get; set; }

		public string TargetPath { get; set; }

		[Output]
		public ITaskItem OutputPackage { get; set; }

		public override bool Execute()
		{
			try
			{
				using (var stream = File.Create(TargetPath))
				{
					BuildPackage(stream);
				}

				OutputPackage = new TaskItem(TargetPath);
				Manifest.CopyMetadataTo(OutputPackage);
					 
				return true;
			}
			catch (Exception ex)
			{
				Log.LogErrorFromException(ex);
				return false;
			}
		}

		// Implementation for testing to avoid I/O
		public Manifest Execute(Stream output)
		{
			BuildPackage(output);

			output.Seek(0, SeekOrigin.Begin);
			using (var reader = new PackageArchiveReader(output))
			{
				return reader.GetManifest();
			}
		}

		public Manifest CreateManifest()
		{
			var metadata = new ManifestMetadata();

			metadata.Id = Manifest.GetMetadata("Id");
			metadata.Version = NuGetVersion.Parse(Manifest.GetMetadata("Version"));

			metadata.Title = Manifest.GetMetadata("Title");
			metadata.Description = Manifest.GetMetadata("Description");
			metadata.Summary = Manifest.GetMetadata("Summary");
			metadata.Language = Manifest.GetMetadata("Language");

			metadata.Copyright = Manifest.GetMetadata("Copyright");
			metadata.RequireLicenseAcceptance = string.IsNullOrEmpty(Manifest.GetMetadata("RequireLicenseAcceptance")) ? false :
				bool.Parse(Manifest.GetMetadata("RequireLicenseAcceptance"));

			if (!string.IsNullOrEmpty(Manifest.GetMetadata("Authors")))
				metadata.Authors = Manifest.GetMetadata("Authors").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			if (!string.IsNullOrEmpty(Manifest.GetMetadata("Owners")))
				metadata.Owners = Manifest.GetMetadata("Owners").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

			if (!string.IsNullOrEmpty(Manifest.GetMetadata("LicenseUrl")))
				metadata.SetLicenseUrl(Manifest.GetMetadata("LicenseUrl"));
			if (!string.IsNullOrEmpty(Manifest.GetMetadata("ProjectUrl")))
				metadata.SetProjectUrl(Manifest.GetMetadata("ProjectUrl"));
			if (!string.IsNullOrEmpty(Manifest.GetMetadata("IconUrl")))
				metadata.SetIconUrl(Manifest.GetMetadata("IconUrl"));

			metadata.ReleaseNotes = Manifest.GetMetadata("ReleaseNotes");
			metadata.Tags = Manifest.GetMetadata("Tags");
			metadata.MinClientVersionString = Manifest.GetMetadata("MinClientVersion");

			var manifest = new Manifest(metadata);

			AddDependencies(manifest);
			AddFiles(manifest);

			return manifest;
		}

		void AddDependencies(Manifest manifest)
		{
			var dependencies = from item in Contents
							   where item.GetMetadata(MetadataName.Kind) == PackageItemKind.Dependency
							   select new Dependency
							   {
								   Id = item.ItemSpec,
								   Version = VersionRange.Parse(item.GetMetadata(MetadataName.Version)),
								   TargetFramework = item.GetTargetFramework()
							   };

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

		void AddFiles(Manifest manifest)
		{
			manifest.Files.AddRange(Contents
				.Where(item => !string.IsNullOrEmpty(item.GetMetadata(MetadataName.PackagePath)))
				.Select(item => new ManifestFile
				{
					Source = item.GetMetadata("FullPath"),
					Target = item.GetMetadata(MetadataName.PackagePath),
				}));
		}

		void BuildPackage(Stream output)
		{
			var builder = new PackageBuilder();
			var manifest = CreateManifest();

			builder.Populate(manifest.Metadata);
			// We don't use PopulateFiles because that performs search expansion, base path 
			// extraction and the like, which messes with our determined files to include.
			// TBD: do we support wilcard-based include/exclude?
			builder.Files.AddRange(manifest.Files.Select(file => 
				new PhysicalPackageFile { SourcePath = file.Source, TargetPath = file.Target }));
			
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
