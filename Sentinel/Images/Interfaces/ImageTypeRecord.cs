namespace Sentinel.Images.Interfaces
{
    public class ImageTypeRecord
    {
        public ImageTypeRecord(string name, ImageQuality quality, string image)
        {
            Name = name;
            Quality = quality;
            Image = image;
        }

        public string Name { get; private set; }

        public ImageQuality Quality { get; private set; }

        public string Image { get; internal set; }
    }
}