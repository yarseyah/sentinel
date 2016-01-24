namespace WpfExtras.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Media;

    [ValueConversion(typeof(Color), typeof(Brush))]
    public class ColourConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                if (parameter != null)
                {
                    switch (parameter.ToString())
                    {
                        case "Window":
                            return SystemColors.WindowBrush;
                        case "WindowText":
                            return SystemColors.WindowTextBrush;
                        default:
                            return null;
                    }
                }

                return null;
            }

            return new SolidColorBrush((Color)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}