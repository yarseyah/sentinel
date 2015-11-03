namespace Sentinel.Extractors.Gui
{
    using System.Windows;

    using Sentinel.Extractors.Interfaces;
    using Sentinel.Services;

    public class RemoveExtractor
        : IRemoveExtractorService
    {
        public void Remove(IExtractor extractor)
        {
            var service = ServiceLocator.Instance.Get<IExtractingService<IExtractor>>();

            if (service != null)
            {
                var prompt = "Are you sure you want to remove the selected extractor?\r\n\r\n" +
                             $"Extractor Name = \"{extractor.Name}\"";

                var result = MessageBox.Show(
                    prompt,
                    "Remove Extractor",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.No);

                if (result == MessageBoxResult.Yes)
                {
                    service.Extractors.Remove(extractor);
                }
            }
        }
    }
}