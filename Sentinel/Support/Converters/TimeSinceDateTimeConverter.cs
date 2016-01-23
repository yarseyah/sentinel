namespace Sentinel.Support.Converters
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Windows.Data;

    public class TimeSinceDateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                DateTime dateTime;
                if (!(value is DateTime))
                {
                    Trace.WriteLine($"Time wasn't passed as DateTime, but as a {value.GetType()}");
                    dateTime = DateTime.Parse((string)value);
                }
                else
                {
                    dateTime = (DateTime)value;
                }

                // adjust the timezone information in the dateTime
                DateTime adjusted = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                TimeSpan elapsed = DateTimeOffset.UtcNow - adjusted;
                return $"{elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2},{elapsed.Milliseconds:D3}";
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}