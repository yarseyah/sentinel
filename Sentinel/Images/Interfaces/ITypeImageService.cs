namespace Sentinel.Images.Interfaces
{
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;
    using System.Windows.Input;

    public interface ITypeImageService
    {
        ICommand Add { get; }

        ICommand Edit { get; }

        [DataMember]
        ObservableCollection<ImageTypeRecord> ImageMappings { get; }

        ICommand Remove { get; }

        int SelectedIndex { get; set; }

        void Register(string type, ImageQuality quality, string image);

        ImageTypeRecord Get(string type, ImageOptions options);
    }
}