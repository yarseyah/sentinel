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
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

#endregion

namespace Sentinel.Converters
{
    [ValueConversion(typeof(Color), typeof(Brush))]
    public class ColourConverter : IValueConverter
    {
        #region IValueConverter Members

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

            return new SolidColorBrush((Color) value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}