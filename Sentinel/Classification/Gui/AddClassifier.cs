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
    public class AddClassifier : IAddClassifyingService
    {
        public void Add()
        {
            AddEditClassifierWindow classifierWindow = new AddEditClassifierWindow();
            using (AddEditClassifier data = new AddEditClassifier(classifierWindow, false))
            {
                classifierWindow.DataContext = data;
                classifierWindow.Owner = Application.Current.MainWindow;
                bool? dialogResult = classifierWindow.ShowDialog();
                if (dialogResult != null && (bool)dialogResult)
                {
                    Classifier classifier = Construct(data);
                    if (classifier != null)
                    {
                        var service = ServiceLocator.Instance.Get<IClassifyingService<IClassifier>>();
                        if (service != null)
                        {
                            service.Classifiers.Add(classifier);
                        }
                    }
                }
            }
        }

        private static Classifier Construct(AddEditClassifier data)
        {
            return new Classifier
            {
                Name = data.Name,
                Type = data.Type,
                Field = data.Field,
                Mode = data.Mode,
                Pattern = data.Pattern,
                Enabled = true
            };
        }
    }
}
