namespace Sentinel.Support.Converters
{
    using Sentinel.Images;

    public class TypeToLargeImageConverter : TypeToImageConverter
    {
        public TypeToLargeImageConverter()
        {
            quality = ImageQuality.Large;
        }
    }
}