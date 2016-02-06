namespace WpfExtras.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    public class VisibilityToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isVisible = (bool)value;

            // If a parameter is supplied, invert the condition.
            if (parameter != null)
            {
                isVisible = !isVisible;
            }

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isVisible = (bool)value;

            // If a parameter is supplied, invert the condition.
            if (parameter != null)
            {
                isVisible = !isVisible;
            }

            return isVisible ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}