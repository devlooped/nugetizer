using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace NuGet.Packaging.VisualStudio
{
	public class ExtendedBooleanToVisiblityConverter : IValueConverter
	{
		public bool VisibleIfFalse { get; set; }
		public bool HiddenInsteadOfCollapsed { get; set; }

		public object Convert (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return (value is bool && ((bool)value != VisibleIfFalse)) ? Visibility.Visible : (HiddenInsteadOfCollapsed ? Visibility.Hidden : Visibility.Collapsed);
		}

		public object ConvertBack (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return (value is Visibility && (Visibility)value == Visibility.Visible) != VisibleIfFalse;
		}
	}
}