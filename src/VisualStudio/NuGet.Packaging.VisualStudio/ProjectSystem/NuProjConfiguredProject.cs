using System;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;

using Microsoft.VisualStudio.ProjectSystem;

namespace NuGet.Packaging.VisualStudio
{
    [Export]
    [AppliesTo(NuProjCapabilities.NuProj)]
    internal sealed class NuProjConfiguredProject
    {
        [Import]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "MEF")]
        public ConfiguredProject ConfiguredProject { get; private set; }

        [Import]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "MEF")]
        public Lazy<NuProjProjectProperties> Properties { get; private set; }
    }
}
