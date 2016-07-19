using System;
using System.ComponentModel.Design;

namespace NuGet.Packaging.VisualStudio
{
	class Commands
	{
		public static readonly CommandID BuildNuGetPackageCommandId =
			new CommandID(new Guid(Guids.CommandSetGuid), 0x0100);

		public static readonly CommandID AddPlatformImplementationCommandId =
			new CommandID(new Guid(Guids.CommandSetGuid), 0x0101);
	}
}