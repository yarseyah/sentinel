namespace Sentinel.Images
{
    using System.Windows;

    using Sentinel.Images.Interfaces;
    using Sentinel.Services;

    public class RemoveTypeImageMapping : IRemoveTypeImage
    {
        public void Remove(ImageTypeRecord typeImageRecord)
        {
            var service = ServiceLocator.Instance.Get<ITypeImageService>();

            if (service != null)
            {
                var prompt = "Are you sure you want to remove the selected image?\r\n\r\n"
                             + $"Image Name = \"{typeImageRecord.Name}\"";

                var result = MessageBox.Show(
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