using Microsoft.VisualStudio.Shell;

namespace NuGet.Packaging.VisualStudio.Theme
{
	public static class Brushes
	{
		// Window
		public static object WindowForegroundKey { get { return VsBrushes.WindowTextKey; } }
		public static object WindowBackgroundKey { get { return VsBrushes.ToolWindowBackgroundKey; } }
		public static object WindowBorderKey { get { return VsBrushes.ToolWindowBorderKey; } }
		public static object WindowKey { get { return VsBrushes.WindowKey; } }
		public static object WindowGrayTextKey { get { return VsBrushes.GrayTextKey; } }

		// Panel
		public static object PanelBackground { get { return VsBrushes.PanelGradientLightKey; } }

		// Editor
		public static object EditorBackgroundKey { get { return VsBrushes.DesignerBackgroundKey; } }

		//ToolBar
		public static object ToolBarBackgroundKey { get { return VsBrushes.CommandBarGradientBeginKey; } }
		public static object ToolBarBorderKey { get { return VsBrushes.CommandBarBorderKey; } }
		public static object ToolBarSeparatorKey { get { return VsBrushes.CommandBarToolBarSeparatorKey; } }

		// EditableControl
		public static object EditableControlBackgroundKey { get { return VsBrushes.ComboBoxBackgroundKey; } }
		public static object EditableControlTextKey { get { return VsBrushes.ToolWindowTextKey; } }
		public static object EditableControlBorderKey { get { return VsBrushes.ComboBoxBorderKey; } }
		public static object EditableControlGlyphKey { get { return VsBrushes.ComboBoxGlyphKey; } }

		// EditableControlDisabled
		public static object EditableControlDisabledBackgroundKey { get { return VsBrushes.ComboBoxDisabledBackgroundKey; } }
		public static object EditableControlDisabledTextKey { get { return VsBrushes.ToolWindowTextKey; } }
		public static object EditableControlDisabledBorderKey { get { return VsBrushes.ComboBoxDisabledBorderKey; } }
		public static object EditableControlDisabledGlyphKey { get { return VsBrushes.ComboBoxDisabledGlyphKey; } }

		public static object EditableControlMouseOverBackgroundKey { get { return VsBrushes.ComboBoxMouseOverBackgroundEndKey; } }
		public static object EditableControlMouseOverTextKey { get { return VsBrushes.ToolWindowTextKey; } }
		public static object EditableControlMouseOverBorderKey { get { return VsBrushes.ComboBoxMouseOverBorderKey; } }
		public static object EditableControlMouseOverGlyphKey { get { return VsBrushes.ComboBoxMouseOverGlyphKey; } }

		// EditableControlMouseDown
		public static object EditableControlMouseDownBackgroundKey { get { return VsBrushes.ComboBoxMouseDownBackgroundKey; } }
		public static object EditableControlMouseDownTextKey { get { return VsBrushes.ToolWindowTextKey; } }
		public static object EditableControlMouseDownBorderKey { get { return VsBrushes.ComboBoxMouseDownBorderKey; } }
		public static object EditableControlMouseDownGlyphKey { get { return VsBrushes.ToolWindowTextKey; } }

		// Info
		public static object InfoBackgroundKey { get { return VsBrushes.InfoBackgroundKey; } }
		public static object InfoTextKey { get { return VsBrushes.InfoTextKey; } }

	}
}
