using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using VSLangProj;
using VSLangProj80;
using VSLangProj150;
using System.Collections.Concurrent;

namespace NuGet.Packaging.VisualStudio
{
	public class ReferencesWrapper : References
	{
		Lazy<VSProject> project;
		Lazy<PackageReferences> packageReferences;
		ConcurrentDictionary<Reference3, ReferenceWrapper> referenceWrappers = new ConcurrentDictionary<Reference3, ReferenceWrapper>();

		public ReferencesWrapper(Lazy<VSProject> project, Lazy<PackageReferences> packageReferences)
		{
			this.project = project;
			this.packageReferences = packageReferences;
		}

		VSProject Project => project.Value;

		public Reference Item(object index) =>
			GetReferenceWrapperOrDefault(Project.References.Item(index));

		Reference GetReferenceWrapperOrDefault(Reference reference)
		{
			var reference3 = reference as Reference3;

			if (reference3 != null)
				return referenceWrappers.GetOrAdd(reference3, r => new ReferenceWrapper(r, packageReferences));

			return reference;
		}

		public IEnumerator GetEnumerator()
		{
			foreach (Reference reference in Project.References)
				yield return GetReferenceWrapperOrDefault(reference);
		}

		public Reference Find(string bstrIdentity) =>
			GetReferenceWrapperOrDefault(Project.References.Find(bstrIdentity));

		public Reference Add(string bstrPath) =>
			GetReferenceWrapperOrDefault(Project.References.Add(bstrPath));

		public Reference AddActiveX(string bstrTypeLibGuid, int lMajorVer = 0, int lMinorVer = 0, int lLocaleId = 0, string bstrWrapperTool = "") =>
			GetReferenceWrapperOrDefault(Project.References.AddActiveX(bstrTypeLibGuid, lMajorVer, lMinorVer, lLocaleId, bstrWrapperTool));

		public Reference AddProject(Project pProject) =>
			GetReferenceWrapperOrDefault(Project.References.AddProject(pProject));

		public DTE DTE => Project.References.DTE;

		public object Parent => Project.References.Parent;

		public Project ContainingProject => Project.References.ContainingProject;

		public int Count => Project.References.Count;
	}
}
