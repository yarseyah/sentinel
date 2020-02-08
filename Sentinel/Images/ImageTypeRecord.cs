namespace Sentinel.Images
{
    using WpfExtras;

    public class ImageTypeRecord : ViewModelBase
    {
        private string name;

        private ImageQuality quality;

        private string image;

        public ImageTypeRecord(string name, ImageQuality quality, string image)
        {
            this.name = name;
            this.quality = quality;
            this.image = image;

            DisplayName = name;
        }

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public ImageQuality Quality
        {
            get
            {
                return quality;
            }

            set
            {
                if (quality != value)
                {
                    quality = value;
                    OnPropertyChanged(nameof(Quality));
                }
            }
        }

        public string Image
        {
            get
            {
                return image;
            }

            set
            {
                if (image != value)
                {
                    image = value;
                    OnPropertyChanged(nameof(Image));
                }
            }
        }
    }
}