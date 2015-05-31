using Sentinel.Images.Interfaces;
using Sentinel.Services;
using System.Windows;

namespace Sentinel.Images
{
    //[Export(typeof(IRemoveTypeImage))]
    public class RemoveTypeImageMapping : IRemoveTypeImage
    {
        public void Remove(ImageTypeRecord typeImageRecord)
        {
            var service = ServiceLocator.Instance.Get<ITypeImageService>();

            if (service != null)
            {
                string prompt = string.Format(
                    "Are you sure you want to remove the selected image?\r\n\r\n" +
                    "Image Name = \"{0}\"",
                    typeImageRecord.Name);

                MessageBoxResult result = MessageBox.Show(
                    prompt,
                    "Remove Image",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.No);

                if (result == MessageBoxResult.Yes)
                {
                    service.ImageMappings.Remove(typeImageRecord);
                }
            }
        }
    }
}