namespace NuGet.Packaging.VisualStudio
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.Composition;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Microsoft.VisualStudio.ProjectSystem;
	using Microsoft.VisualStudio.ProjectSystem.Properties;
	using Microsoft.VisualStudio.ProjectSystem.Utilities;

	[Export]
	[AppliesTo(NuProjCapabilities.NuProj)]
	internal partial class ProjectProperties : StronglyTypedPropertyAccess
	{
		[ImportingConstructor]
		public ProjectProperties(ConfiguredProject configuredProject)
			: base(configuredProject)
		{ }

		public ProjectProperties(ConfiguredProject configuredProject, string file, string itemType, string itemName)
			: base(configuredProject, file, itemType, itemName)
		{ }

		public ProjectProperties(ConfiguredProject configuredProject, IProjectPropertiesContext projectPropertiesContext)
			: base(configuredProject, projectPropertiesContext)
		{ }

		public ProjectProperties(ConfiguredProject configuredProject, UnconfiguredProject unconfiguredProject)
			: base(configuredProject, unconfiguredProject)
		{ }
	}
}
