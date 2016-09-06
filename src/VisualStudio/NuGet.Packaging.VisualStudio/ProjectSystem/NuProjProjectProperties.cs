using System.ComponentModel.Composition;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Properties;

namespace NuGet.Packaging.VisualStudio
{
    /// <summary>
    /// Provides rule-based property access.
    /// </summary>
    [Export]
    [AppliesTo(NuProjCapabilities.NuProj)]
    internal partial class NuProjProjectProperties : StronglyTypedPropertyAccess
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectProperties"/> class.
        /// </summary>
        [ImportingConstructor]
        public NuProjProjectProperties([Import] ConfiguredProject configuredProject)
            : base(configuredProject)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectProperties"/> class.
        /// </summary>
        public NuProjProjectProperties(ConfiguredProject configuredProject, string file, string itemType, string itemName)
            : base(configuredProject, file, itemType, itemName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectProperties"/> class.
        /// </summary>
        public NuProjProjectProperties(ConfiguredProject configuredProject, IProjectPropertiesContext projectPropertiesContext)
            : base(configuredProject, projectPropertiesContext)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectProperties"/> class.
        /// </summary>
        public NuProjProjectProperties(ConfiguredProject configuredProject, UnconfiguredProject unconfiguredProject)
            : base(configuredProject, unconfiguredProject)
        {
        }
    }
}
