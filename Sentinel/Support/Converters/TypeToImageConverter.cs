namespace Sentinel.Support.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using Sentinel.Images;
    using Sentinel.Images.Interfaces;
    using Sentinel.Services;

    [ValueConversion(typeof(string), typeof(ImageSource))]
    public class TypeToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var imageService = ServiceLocator.Instance.Get<ITypeImageService>();
            var valueAsString = value as string;

            if (!string.IsNullOrWhiteSpace(valueAsString))
            {
                var record = imageService?.Get(valueAsString, Quality, true, true);

                if (!string.IsNullOrEmpty(record?.Image))
                {
                    var image = new BitmapImage();

                    image.BeginInit();
                    image.UriSource = new Uri(record.Image, UriKind.RelativeOrAbsolute);
                    image.EndInit();

                    return image;
                }
            }

            return null;
        }

        protected ImageQuality Quality { get; set; } = ImageQuality.BestAvailable;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}