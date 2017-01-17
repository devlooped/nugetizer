using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using VSLangProj;
using VSLangProj150;
using VSLangProj80;

namespace NuGet.Packaging.VisualStudio
{
	class ReferenceWrapper : Reference6
	{
		Reference3 reference;
		Lazy<PackageReferences> packageReferences;

		public ReferenceWrapper(Reference3 reference, Lazy<PackageReferences> packageReferences)
		{
			this.reference = reference;
			this.packageReferences = packageReferences;
		}

		public void Remove() => reference.Remove();

		public void GetMetadata(Array parrbstrDesiredMetadata, out Array pparrbstrMetadataElements, out Array pparrbstrMetadataValues)
		{
			string version;
			packageReferences.Value.TryGetReference(Name, parrbstrDesiredMetadata, out version, out pparrbstrMetadataElements, out pparrbstrMetadataValues);
		}

		public void AddOrUpdateMetadata(Array parrbstrMetadataElements, Array parrbstrMetadataValues)
		{
			packageReferences.Value.AddOrUpdate(Name, Version, parrbstrMetadataElements, parrbstrMetadataValues);
		}

		public DTE DTE => reference.DTE;

		public References Collection => reference.Collection;

		public Project ContainingProject => reference.ContainingProject;

		public string Name => reference.Name;

		public prjReferenceType Type => reference.Type;

		public string Identity => reference.Identity;

		public string Path => reference.Path;

		public string Description => reference.Description;

		public string Culture => reference.Culture;

		public int MajorVersion => reference.MajorVersion;

		public int MinorVersion => reference.MinorVersion;

		public int RevisionNumber => reference.RevisionNumber;

		public int BuildNumber => reference.BuildNumber;

		public bool StrongName => reference.StrongName;

		public Project SourceProject => reference.SourceProject;

		public bool CopyLocal
		{
			get { return reference.CopyLocal; }
			set { reference.CopyLocal = value; }
		}

		public dynamic get_Extender(string ExtenderName) => reference.Extender[ExtenderName];

		public dynamic ExtenderNames => reference.ExtenderNames;

		public string ExtenderCATID => reference.ExtenderCATID;

		public string PublicKeyToken => reference.PublicKeyToken;

		public string Version => reference.Version;

		public string RuntimeVersion => reference.RuntimeVersion;

		public bool SpecificVersion
		{
			get { return reference.SpecificVersion; }
			set { reference.SpecificVersion = value; }
		}

		public string SubType
		{
			get { return reference.SubType; }
			set { reference.SubType = value; }
		}

		public bool Isolated
		{
			get { return reference.Isolated; }
			set { reference.Isolated = value; }
		}

		public string Aliases
		{
			get { return reference.Aliases; }
			set { reference.Aliases = value; }
		}

		public uint RefType => reference.RefType;

		public bool AutoReferenced => reference.AutoReferenced;

		public bool Resolved => reference.Resolved;

		public bool EmbedInteropTypes { get; set; }

		public Array ExpandedSdkReferences => null;

		public Reference Group => null;
	}
}
