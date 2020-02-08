namespace Sentinel.Classification.Gui
{
    using System.Windows;

    using Sentinel.Classification.Interfaces;
    using Sentinel.Services;

    public class RemoveClassifier : IRemoveClassifyingService
    {
        public void Remove(IClassifier classifier)
        {
            var service = ServiceLocator.Instance.Get<IClassifyingService<IClassifier>>();

            if (service != null)
            {
                var prompt =
                    "Are you sure you want to remove the selected classifier?\r\n\r\n" +
                    $"Classifier Name = \"{classifier.Name}\"";

                var result = MessageBox.Show(
                    prompt,
                    "Remove Extractor",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.No);

                if (result == MessageBoxResult.Yes)
                {
                    service.Classifiers.Remove(classifier);
                }
            }
        }
    }
}