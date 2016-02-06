namespace Sentinel.Images.Interfaces
{
    public class ImageOptions
    {
        public ImageQuality Quality { get; set; } = ImageQuality.BestAvailable;

        public bool AcceptLowerQuality { get; set; } = true;

        public bool ImageMustExist { get; set; } = false;
    }
}