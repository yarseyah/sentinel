namespace Sentinel.Support.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    using Common.Logging;

    using NodaTime;

    using Sentinel.Interfaces;

    public class DateTimePreferenceConverter : IValueConverter
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public IUserPreferences Preferences { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Preferences = parameter as IUserPreferences;

            if (Preferences == null)
            {
                Log.Error("Parameter must be an IUserPreferences");
                throw new ArgumentException("Parameter must be an instance of IUserPreferences", nameof(parameter));
            }

            var message = value as ILogEntry;

            if (message == null)
            {
                Log.Warn("Not supplied an ILogEntry as the value parameter");
                return string.Empty;
            }

            object displayDateTime = null;
            if (Preferences.DateSourceOption == 0)
            {
                message.MetaData.TryGetValue("ReceivedUtcTime", out displayDateTime);
            }
            else if (Preferences.DateSourceOption == 1)
            {
                message.MetaData.TryGetValue("ReceivedLocalTime", out displayDateTime);
            }

            // Fallback if message does not contain meta-data.
            var dt = (DateTime)(displayDateTime ?? message.DateTime);

            if (dt.Kind == DateTimeKind.Utc)
            {
                var defaultTimeZone = DateTimeZoneProviders.Tzdb.GetSystemDefault();
                var global = new ZonedDateTime(Instant.FromDateTimeUtc(dt), defaultTimeZone);
                return global.ToString(
                    GetDateDisplayFormat(Preferences.SelectedDateOption, true),
                    CultureInfo.CurrentCulture);
            }

            var local = LocalDateTime.FromDateTime(dt);
            return local.ToString(GetDateDisplayFormat(Preferences.SelectedDateOption), CultureInfo.CurrentCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static string GetDateDisplayFormat(int setting, bool withTimeZone = false)
        {
            var dateFormat = withTimeZone ? "yyyy'-'MM'-'dd'T'HH':'mm':'ss;FFFFFFF x" : "yyyy'-'MM'-'dd'T'HH':'mm':'ss;FFFFFFF";
            switch (setting)
            {
                case 1:
                    dateFormat = withTimeZone ? "dd/MM/yyyy HH:mm:ss x" : "dd/MM/yyyy HH:mm:ss";
                    break;
                case 2:
                    dateFormat = withTimeZone ? "dddd, d MMM yyyy, HH:mm:ss x" : "dddd, d MMM yyyy, HH:mm:ss";
                    break;
                case 3:
                    dateFormat = withTimeZone ? "HH:mm:ss x" : "HH:mm:ss";
                    break;
                case 4:
                    dateFormat = withTimeZone ? "HH:mm:ss,fff x" : "HH:mm:ss,fff";
                    break;
            }
            return dateFormat;
        }
    }
}