namespace Sentinel.Support.Converters
{
    using Sentinel.Images;

    public class TypeToMediumImageConverter : TypeToImageConverter
    {
        public TypeToMediumImageConverter()
        {
            Quality = ImageQuality.Medium;
        }
    }
}