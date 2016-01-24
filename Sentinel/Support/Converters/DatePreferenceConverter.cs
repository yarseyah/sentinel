namespace Sentinel.Support.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;

    using Common.Logging;

    using NodaTime;

    using Sentinel.Interfaces;
    using Sentinel.Interfaces.CodeContracts;

    public class DatePreferenceConverter : IValueConverter
    {
        private static readonly ILog Log = LogManager.GetLogger<DatePreferenceConverter>();

        private IUserPreferences Preferences { get; set; }

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
            if (Preferences.UseArrivalDateTime)
            {
                message.MetaData.TryGetValue("ReceivedTime", out displayDateTime);
            }

            // Fallback if message does not contain meta-data.
            // TODO: safely handle the meta-data dateTime not being a date-time!
            var dt = (DateTime?)displayDateTime ?? message.DateTime;

            // TODO: make a time selection option....
            if (dt.Kind == DateTimeKind.Utc && Preferences.ConvertUtcTimesToLocalTimeZone)
            {
                var defaultTimeZone = DateTimeZoneProviders.Tzdb.GetSystemDefault();
                var global = new ZonedDateTime(Instant.FromDateTimeUtc(dt), defaultTimeZone);
                return
                    global.ToString(
                        GetDateDisplayFormat(Preferences.SelectedDateOption, Preferences.DateFormatOptions),
                        CultureInfo.CurrentCulture);
            }

            var local = LocalDateTime.FromDateTime(dt);
            return local.ToString(
                GetDateDisplayFormat(Preferences.SelectedDateOption, Preferences.DateFormatOptions),
                CultureInfo.CurrentCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static string GetDateDisplayFormat(int setting, IEnumerable<string> settings)
        {
            settings.ThrowIfNull(nameof(settings));

            var dateFormatSource = settings.ElementAt(setting);

            // Need to quote special characters, this will only happen when changing formats, so don't need to be too clever.
            return dateFormatSource.Replace("-", "'-'").Replace(":", "':'");
        }
    }
}