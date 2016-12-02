using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;
using System.Collections.Immutable;
using System.Threading;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.ProjectSystem;
using System.IO;
using Microsoft.VisualStudio.ProjectSystem.Build;

namespace NuGet.Packaging.VisualStudio
{
	[Export(typeof(IProjectGlobalPropertiesProvider))]
	//[AppliesTo(NuProjCapabilities.NuProj)]
	[AppliesTo("")]
	class GlobalPropertiesProvider : StaticGlobalPropertiesProviderBase
	{
		[ImportingConstructor]
		internal GlobalPropertiesProvider(IProjectService projectService)
			: base(projectService.Services)
		{ }

		public override Task<IImmutableDictionary<string, string>> GetGlobalPropertiesAsync(CancellationToken cancellationToken) =>
			Task.FromResult<IImmutableDictionary<string, string>>(
				Empty.PropertiesMap.SetItem(
					Constants.NuGetAuthoringPathPropertyName,
					Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), "Targets")));
	}
}