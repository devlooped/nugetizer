using System.ComponentModel.Composition;

using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.VS;

namespace NuGet.Packaging.VisualStudio
{
    [Export]
    [AppliesTo(NuProjCapabilities.NuProj)]
    [ProjectTypeRegistration(Guids.ProjectTypeGuid,
                             "NuGet",
                             "#2",
                             Constants.ProjectExtension,
                             Constants.Language,
                             Guids.PackageGuid,
                             PossibleProjectExtensions = Constants.ProjectExtension,
                             ProjectTemplatesDir = @"..\..\Templates\Projects\NuProj",
                             Capabilities = NuProjCapabilities.DefaultCapabilities)]
    internal sealed class NuProjUnconfiguredProject
    {
        [Import]
        public UnconfiguredProject UnconfiguredProject { get; private set; }

        [Import]
        public ActiveConfiguredProject<NuProjConfiguredProject> NuProjActiveConfiguredProject { get; private set; }

        [Import]
        public ConfiguredProject ActiveConfiguredProject
        {
            get { return NuProjActiveConfiguredProject.Value.ConfiguredProject; }
        }
    }
}