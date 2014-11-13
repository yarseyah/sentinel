using Sentinel.Support.Mvvm;

namespace Sentinel.Images
{
    public class ImageTypeRecord : ViewModelBase
    {
        private string _name;
        private ImageQuality _quality;
        private string _image;

        public ImageTypeRecord(string name, ImageQuality quality, string image)
        {
            _name = name;
            _quality = quality;
            _image = image;
        }

        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public ImageQuality Quality
        {
            get
            {
                return _quality;
            }
            set
            {
                if (_quality != value)
                {
                    _quality = value;
                    OnPropertyChanged("Quality");
                }
            }
        }

        public string Image
        {
            get
            {
                return _image;
            }
            set
            {
                if (_image != value)
                {
                    _image = value;
                    OnPropertyChanged("Image");
                }
            }
        }
    }
}