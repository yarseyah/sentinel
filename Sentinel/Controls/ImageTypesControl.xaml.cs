namespace Sentinel.Controls
{
    using System.Windows.Controls;

    using Sentinel.Images.Interfaces;
    using Sentinel.Services;

    /// <summary>
    /// Interaction logic for ImageTypesControl.xaml.
    /// </summary>
    public partial class ImageTypesControl : UserControl
    {
        public ImageTypesControl()
        {
            InitializeComponent();
            Images = ServiceLocator.Instance.Get<ITypeImageService>();
            DataContext = this;
        }

        public ITypeImageService Images { get; private set; }
    }
}