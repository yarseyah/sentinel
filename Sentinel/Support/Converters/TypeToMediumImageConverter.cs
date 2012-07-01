namespace Sentinel.Support.Converters
{
    using Sentinel.Images;

    public class TypeToMediumImageConverter : TypeToImageConverter
    {
        public TypeToMediumImageConverter()
        {
            this.quality = ImageQuality.Medium;
        }
    }
}