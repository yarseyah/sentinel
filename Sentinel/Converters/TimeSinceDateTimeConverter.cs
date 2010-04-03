#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

#endregion

namespace Sentinel.Converters
{
    public class TimeSinceDateTimeConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                DateTime dateTime;
                if (!(value is DateTime))
                {
                    Trace.WriteLine(string.Format("Time wasn't passed as DateTime, but as a {0}", value.GetType()));
                    dateTime = DateTime.Parse((string) value);
                }
                else
                {
                    dateTime = (DateTime) value;
                }

                // adjust the timezone information in the dateTime
                DateTime adjusted = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                TimeSpan elapsed = DateTimeOffset.UtcNow - adjusted;
                return string.Format(
                    "{0:D2}:{1:D2}:{2:D2},{3:D3}",
                    elapsed.Hours,
                    elapsed.Minutes,
                    elapsed.Seconds,
                    elapsed.Milliseconds);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}