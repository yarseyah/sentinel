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
using Sentinel.Images.Interfaces;
using Sentinel.Services;

#endregion

namespace Sentinel.Support.Converters
{
    using Sentinel.Images;

    [ValueConversion(typeof(string), typeof(ImageSource))]
    public class TypeToImageConverter : IValueConverter
    {
        protected ImageQuality quality = ImageQuality.BestAvailable;

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var imageService = ServiceLocator.Instance.Get<ITypeImageService>();
            var valueAsString = value as string;

            if (!string.IsNullOrWhiteSpace(valueAsString))
            {
                if (imageService != null)
                {
                    var record = imageService.Get(valueAsString, quality, true, true);

                    if (record != null && !string.IsNullOrEmpty(record.Image))
                    {
                        var image = new BitmapImage();

                        image.BeginInit();
                        image.UriSource = new Uri(record.Image, UriKind.RelativeOrAbsolute);
                        image.EndInit();

                        return image;
                    }
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