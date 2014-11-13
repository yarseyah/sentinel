using Sentinel.Classification.Interfaces;
using Sentinel.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Sentinel.Classification.Gui
{
    public class RemoveClassifier : IRemoveClassifyingService
    {
        public void Remove(IClassifier classifier)
        {
            var service = ServiceLocator.Instance.Get<IClassifyingService<IClassifier>>();

            if (service != null)
            {
                string prompt = string.Format(
                    "Are you sure you want to remove the selected classifier?\r\n\r\n" +
                    "Classifier Name = \"{0}\"",
                    classifier.Name);

                MessageBoxResult result = MessageBox.Show(
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
