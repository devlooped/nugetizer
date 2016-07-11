using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Designers;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using Microsoft.VisualStudio.ProjectSystem.Utilities.Designers;

namespace NuGet.Packaging.VisualStudio
{
	[Export(typeof(IProjectTreeModifier))]
	[AppliesTo(NuProjCapabilities.NuProj)]
	internal sealed class NuProjProjectTreeModifier : IProjectTreeModifier
	{
		[Import]
		public UnconfiguredProject UnconfiguredProject { get; set; }

		public IProjectTree ApplyModifications(IProjectTree tree, IProjectTreeProvider projectTreeProvider)
		{
			if (tree.Capabilities.Contains(ProjectTreeCapabilities.ProjectRoot))
				tree = tree.SetIcon(KnownMonikers.NuGet.ToProjectSystemType());

			return tree;
		}
	}
}
