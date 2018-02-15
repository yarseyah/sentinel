namespace Sentinel.Support.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows.Data;
    using Sentinel.Interfaces.CodeContracts;

    public class MetaDataParameterConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            values.ThrowIfNull(nameof(values));

            if (values.Length != 2)
            {
                throw new ArgumentException("Must have two values", nameof(values));
            }

            var meta = values[0] as IDictionary<string, object>;
            if (meta == null)
            {
                throw new ArgumentException("First value in values needs to be IDictionary<string, object>");
            }

            var member = values[1] as string;
            if (member == null)
            {
                throw new ArgumentException("Second value in values[] must be a string");
            }

            if (meta.TryGetValue(member, out var metaDataValue))
            {
                return metaDataValue;
            }

            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}