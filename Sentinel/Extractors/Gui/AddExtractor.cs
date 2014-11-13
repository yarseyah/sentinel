using Sentinel.Extractors.Interfaces;
using Sentinel.Interfaces;
using Sentinel.Services;
using Sentinel.Support.Converters;
using System.Linq;
using System.Windows;

namespace Sentinel.Extractors.Gui
{
    public class AddExtractor
        : IAddExtractorService
    {
        #region IAddExtractorService Members

        public void Add()
        {
            AddEditExtractorWindow extractorWindow = new AddEditExtractorWindow();
            using (AddEditExtractor data = new AddEditExtractor(extractorWindow, false))
            {
                extractorWindow.DataContext = data;
                extractorWindow.Owner = Application.Current.MainWindow;
                bool? dialogResult = extractorWindow.ShowDialog();
                if (dialogResult != null && (bool)dialogResult)
                {
                    Extractor extractor = Construct(data);
                    if (extractor != null)
                    {
                        var service = ServiceLocator.Instance.Get<IExtractingService<IExtractor>>();
                        if (service != null)
                        {
                            service.Extractors.Add(extractor);
                        }
                    }
                }
            }
        }

        #endregion

        private static Extractor Construct(AddEditExtractor data)
        {
            return new Extractor
            {
                Name = data.Name,
                Field = data.Field,
                Mode = data.Mode,
                Pattern = data.Pattern,
                Enabled = true
            };
        }
    }
}