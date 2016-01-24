namespace WpfExtras.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class BooleanToDisabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null || (value is bool && !(bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return value is bool && !(bool)value;
            }

            return false;
        }
    }
}