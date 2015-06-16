namespace WpfExtras.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    public class VisibilityToHiddenConverter : IValueConverter
    {
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
    }
}