namespace Sentinel.Images
{
    using System;
    using System.Windows;

    using Sentinel.Images.Controls;
    using Sentinel.Images.Interfaces;
    using Sentinel.Services;

    using WpfExtras;

    public class AddTypeImageService : ViewModelBase, IAddTypeImage
    {
        private AddImageWindow addImageWindow;

        public void Add()
        {
            if (addImageWindow != null)
            {
                MessageBox.Show(
                    "Only able to have one add image dialog open at a time!",
                    "Add image",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            else
            {
                addImageWindow = new AddImageWindow { DataContext = this, Owner = Application.Current.MainWindow };

                var imageService = ServiceLocator.Instance.Get<ITypeImageService>();
                var data = new AddEditTypeImageViewModel(addImageWindow, imageService, true);
                var dialogResult = addImageWindow.ShowDialog();

                if (dialogResult == true)
                {
                    if (imageService != null)
                    {
                        var imageSize = (ImageQuality)Enum.Parse(typeof(ImageQuality), data.Size);
                        imageService.Register(data.Type, imageSize, data.FileName);
                    }
                }

                addImageWindow = null;
            }
        }
    }
}