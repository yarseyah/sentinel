namespace Sentinel.Support.Converters
{
    using Sentinel.Images;

    public class TypeToSmallImageConverter : TypeToImageConverter
    {
        public TypeToSmallImageConverter()
        {
            Quality = ImageQuality.Small;
        }
    }
}