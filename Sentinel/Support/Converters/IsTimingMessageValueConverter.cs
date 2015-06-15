namespace Sentinel.Support.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class IsTimingMessageValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is string && (value as string).StartsWith("[SimulationTime]");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}