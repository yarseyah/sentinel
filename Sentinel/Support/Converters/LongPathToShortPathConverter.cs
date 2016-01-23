namespace Sentinel.Support.Converters
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Windows.Data;

    [ValueConversion(typeof(string), typeof(string))]
    public class LongPathToShortPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var valueString = (string)value;
            var pathParts = valueString.Split(Path.DirectorySeparatorChar);

            if (pathParts.Length > 6)
            {
                var retData = new StringBuilder();
                retData.Append(Path.Combine(pathParts[0], pathParts[1], pathParts[3]));
                retData.Append("...");
                retData.Append(Path.Combine(pathParts[pathParts.Length - 2], pathParts[pathParts.Length - 1]));

                return retData.ToString();
            }
            else
            {
                return valueString;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
