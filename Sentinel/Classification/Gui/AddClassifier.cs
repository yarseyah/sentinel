namespace Sentinel.Classification.Gui
{
    using System.Windows;

    using Sentinel.Classification.Interfaces;
    using Sentinel.Services;

    public class AddClassifier : IAddClassifyingService
    {
        public void Add()
        {
            var classifierWindow = new AddEditClassifierWindow();
            using (var data = new AddEditClassifier(classifierWindow, false))
            {
                classifierWindow.DataContext = data;
                classifierWindow.Owner = Application.Current.MainWindow;

                var dialogResult = classifierWindow.ShowDialog();
                if (dialogResult != null && (bool)dialogResult)
                {
                    var classifier = Construct(data);
                    if (classifier != null)
                    {
                        var service = ServiceLocator.Instance.Get<IClassifyingService<IClassifier>>();
                        service?.Classifiers.Add(classifier);
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
                           Enabled = true,
                       };
        }
    }
}