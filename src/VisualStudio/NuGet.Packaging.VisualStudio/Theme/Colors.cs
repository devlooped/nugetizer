using Microsoft.VisualStudio.Shell;

namespace NuGet.Packaging.VisualStudio.Theme
{
	public static class Colors
	{
		// Window
		public static object WindowForegroundKey { get { return VsColors.WindowTextKey; } }
		public static object WindowBackgroundKey { get { return VsColors.ToolWindowBackgroundKey; } }
		public static object WindowBorderKey { get { return VsColors.ToolWindowBorderKey; } }
		public static object WindowKey { get { return VsColors.WindowKey; } }
		public static object WindowGrayTextKey { get { return VsColors.GrayTextKey; } }

		// Panel
		public static object PanelBackground { get { return VsColors.PanelGradientLightKey; } }

		// Editor
		public static object EditorBackgroundKey { get { return VsColors.DesignerBackgroundKey; } }

		//ToolBar
		public static object ToolBarBackgroundKey { get { return VsColors.CommandBarGradientBeginKey; } }
		public static object ToolBarBorderKey { get { return VsColors.CommandBarBorderKey; } }
		public static object ToolBarSeparatorKey { get { return VsColors.CommandBarToolBarSeparatorKey; } }

		// EditableControl
		public static object EditableControlBackgroundKey { get { return VsColors.ComboBoxBackgroundKey; } }
		public static object EditableControlTextKey { get { return VsColors.ToolWindowTextKey; } }
		public static object EditableControlBorderKey { get { return VsColors.ComboBoxBorderKey; } }
		public static object EditableControlGlyphKey { get { return VsColors.ComboBoxGlyphKey; } }

		// EditableControlDisabled
		public static object EditableControlDisabledBackgroundKey { get { return VsColors.ComboBoxDisabledBackgroundKey; } }
		public static object EditableControlDisabledTextKey { get { return VsColors.ToolWindowTextKey; } }
		public static object EditableControlDisabledBorderKey { get { return VsColors.ComboBoxDisabledBorderKey; } }
		public static object EditableControlDisabledGlyphKey { get { return VsColors.ComboBoxDisabledGlyphKey; } }

		public static object EditableControlMouseOverBackgroundKey { get { return VsColors.ComboBoxMouseOverBackgroundEndKey; } }
		public static object EditableControlMouseOverTextKey { get { return VsColors.ToolWindowTextKey; } }
		public static object EditableControlMouseOverBorderKey { get { return VsColors.ComboBoxMouseOverBorderKey; } }
		public static object EditableControlMouseOverGlyphKey { get { return VsColors.ComboBoxMouseOverGlyphKey; } }

		// EditableControlMouseDown
		public static object EditableControlMouseDownBackgroundKey { get { return VsColors.ComboBoxMouseDownBackgroundKey; } }
		public static object EditableControlMouseDownTextKey { get { return VsColors.ToolWindowTextKey; } }
		public static object EditableControlMouseDownBorderKey { get { return VsColors.ComboBoxMouseDownBorderKey; } }
		public static object EditableControlMouseDownGlyphKey { get { return VsColors.ToolWindowTextKey; } }
		
		// Info
		public static object InfoBackgroundKey { get { return VsColors.InfoBackgroundKey; } }
		public static object InfoTextKey { get { return VsColors.InfoTextKey; } }
	}
}
