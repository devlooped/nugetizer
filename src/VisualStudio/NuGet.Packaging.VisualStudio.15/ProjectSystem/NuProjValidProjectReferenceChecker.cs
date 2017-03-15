using System;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Immutable;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.References;
using System.Collections.Generic;

namespace NuGet.Packaging.VisualStudio
{
	[Export(typeof(IValidProjectReferenceChecker))]
	[Order(1000)]
	[AppliesTo(NuProjCapabilities.NuProj)]
	public class NuProjValidProjectReferenceChecker : IValidProjectReferenceChecker
	{
		// This import must be present so that this part applies to a specific project.
		[Import]
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
		public UnconfiguredProject UnconfiguredProject { get; set; }

		public Task<SupportedCheckResult> CanAddProjectReferenceAsync(object referencedProject) =>
			Task.FromResult(CanAddProjectReference(referencedProject));

		public Task<CanAddProjectReferencesResult> CanAddProjectReferencesAsync(IImmutableSet<object> referencedProjects) =>
			Task.FromResult(
				new CanAddProjectReferencesResult(
					ImmutableDictionary<object, SupportedCheckResult>.Empty.AddRange(
						referencedProjects.Select(x =>
							new KeyValuePair<object, SupportedCheckResult>(x, CanAddProjectReference(x)))
						), null));

		public Task<SupportedCheckResult> CanBeReferencedAsync(object referencingProject) =>
			Task.FromResult(SupportedCheckResult.Supported);

		SupportedCheckResult CanAddProjectReference(object referencedProject) => SupportedCheckResult.Supported;
	}
}