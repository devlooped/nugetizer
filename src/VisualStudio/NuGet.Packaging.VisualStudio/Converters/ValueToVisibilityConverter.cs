using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace NuGet.Packaging.VisualStudio
{
	public class ValueToVisibilityConverter : IValueConverter
	{
		public bool VisibleIfFalse { get; set; }
		public bool HiddenInsteadOfCollapsed { get; set; }

		// Adapts to the given type taking as true:
		// string if it's not null or empty.
		// other type if it's not null.
		public object Convert (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			bool boolValue = true;
			if (value is bool) {
				boolValue = (bool)value;
			} else if (value is string) {
				boolValue = !string.IsNullOrEmpty (value as string);
			} else boolValue = value != null;

			return ((boolValue != VisibleIfFalse) ? Visibility.Visible : (HiddenInsteadOfCollapsed ? Visibility.Hidden : Visibility.Collapsed));
		}

		// Always return a boolean
		public object ConvertBack (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return (value is Visibility && (Visibility)value == Visibility.Visible) != VisibleIfFalse;
		}
	}
}