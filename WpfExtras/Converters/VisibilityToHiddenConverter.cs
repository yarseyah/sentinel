#region License
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
#endregion

namespace WpfExtras.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    public class VisibilityToHiddenConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isVisible = (bool)value;

            if (parameter != null)
            {
                return isVisible ? Visibility.Hidden : Visibility.Visible;
            }

            return isVisible ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isVisible = (bool)value;
            return isVisible ? Visibility.Hidden : Visibility.Visible;
        }

        #endregion
    }
}