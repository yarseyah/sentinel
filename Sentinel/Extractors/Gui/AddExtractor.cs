namespace Sentinel.Extractors.Gui
{
    using System.Windows;

    using Sentinel.Extractors.Interfaces;
    using Sentinel.Services;

    public class AddExtractor : IAddExtractorService
    {
        public void Add()
        {
            var extractorWindow = new AddEditExtractorWindow();
            using (var data = new AddEditExtractor(extractorWindow, false))
            {
                extractorWindow.DataContext = data;
                extractorWindow.Owner = Application.Current.MainWindow;

                var dialogResult = extractorWindow.ShowDialog();
                if (dialogResult == null || !(bool)dialogResult)
                {
                    return;
                }

                var extractor = Construct(data);
                if (extractor == null)
                {
                    return;
                }

                var service = ServiceLocator.Instance.Get<IExtractingService<IExtractor>>();
                service?.Extractors.Add(extractor);
            }
        }

        private static Extractor Construct(AddEditExtractor data)
        {
            return new Extractor
                       {
                           Name = data.Name,
                           Field = data.Field,
                           Mode = data.Mode,
                           Pattern = data.Pattern,
                           Enabled = true,
                       };
        }
    }
}