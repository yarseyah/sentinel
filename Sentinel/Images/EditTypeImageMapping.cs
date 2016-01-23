namespace Sentinel.Images
{
    using System.Diagnostics;
    using System.Windows;

    using Sentinel.Images.Controls;
    using Sentinel.Images.Interfaces;
    using Sentinel.Services;

    public class EditTypeImageMapping : IEditTypeImage
    {
        public void Edit(ImageTypeRecord imageTypeRecord)
        {
            Debug.Assert(imageTypeRecord != null, "ImageTypeRecord must be supplied to allow editing.");

            var service = ServiceLocator.Instance.Get<ITypeImageService>();

            var window = new AddImageWindow();
            var data = new AddEditTypeImageViewModel(window, service, false);
            window.DataContext = data;
            window.Owner = Application.Current.MainWindow;

            data.Type = imageTypeRecord.Name;
            data.Size = imageTypeRecord.Quality.ToString();
            data.FileName = imageTypeRecord.Image;

            bool? dialogResult = window.ShowDialog();

            if (dialogResult != null && (bool)dialogResult)
            {
                imageTypeRecord.Image = data.FileName;
            }
        }
    }
}