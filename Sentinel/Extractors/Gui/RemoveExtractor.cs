using Sentinel.Extractors.Interfaces;
using Sentinel.Services;
using System.Windows;

namespace Sentinel.Extractors.Gui
{
    public class RemoveExtractor
        : IRemoveExtractorService
    {
        #region IRemoveExtractorService Members

        public void Remove(IExtractor extractor)
        {
            var service = ServiceLocator.Instance.Get<IExtractingService<IExtractor>>();

            if (service != null)
            {
                string prompt = string.Format(
                    "Are you sure you want to remove the selected extractor?\r\n\r\n" +
                    "Extractor Name = \"{0}\"",
                    extractor.Name);

                MessageBoxResult result = MessageBox.Show(
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

        #endregion IRemoveExtractorService Members
    }
}