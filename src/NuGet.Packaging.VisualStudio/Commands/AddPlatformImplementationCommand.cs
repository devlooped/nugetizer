using System.ComponentModel.Composition;

namespace NuGet.Packaging.VisualStudio
{
	[Export(typeof(DynamicCommand))]
	class AddPlatformImplementationCommand : DynamicCommand
	{
		readonly ISelectionService selectionService;
		readonly IDialogService dialogService;

		[ImportingConstructor]
		public AddPlatformImplementationCommand(ISelectionService selectionService, IDialogService dialogService)
			: base(Commands.AddPlatformImplementationCommandId)
		{
			this.selectionService = selectionService;
			this.dialogService = dialogService;
		}

		protected override void Execute()
		{
		}
	}
}