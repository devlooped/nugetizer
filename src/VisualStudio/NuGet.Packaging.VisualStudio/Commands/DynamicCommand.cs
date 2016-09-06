using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;

namespace NuGet.Packaging.VisualStudio
{
	public abstract class DynamicCommand : OleMenuCommand
	{
		public DynamicCommand(CommandID id)
			: base(OnExecute, id)
		{
			BeforeQueryStatus += new EventHandler(OnBeforeQueryStatus);
		}

		void OnBeforeQueryStatus(object sender, EventArgs e)
		{
			CanExecute(sender as OleMenuCommand);
		}

		static void OnExecute(object sender, EventArgs e)
		{
			var command = sender as DynamicCommand;
			if (command != null)
				command.Execute();
		}

		protected virtual void CanExecute(OleMenuCommand command)
		{
			command.Enabled = command.Visible = command.Supported = true;
		}

		protected abstract void Execute();
	}
}