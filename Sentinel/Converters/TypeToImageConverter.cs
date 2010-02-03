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
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Sentinel.Images;
using Sentinel.Services;

#endregion

namespace Sentinel.Converters
{
    [ValueConversion(typeof(string), typeof(ImageSource))]
    public class TypeToImageConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ITypeImageService imageService = ServiceLocator.Instance.Get<ITypeImageService>();
            if (imageService != null)
            {
                string imageName = imageService.ImageMappings
                    .Where(e => e.Key == ((string) value).ToUpper())
                    .FirstOrDefault()
                    .Value;

                if (!string.IsNullOrEmpty(imageName))
                {
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.UriSource = new Uri(imageName, UriKind.Relative);
                    image.EndInit();
                    return image;
                }
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