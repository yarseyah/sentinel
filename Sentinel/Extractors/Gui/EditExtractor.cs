using Sentinel.Extractors.Interfaces;
using Sentinel.Interfaces;
using Sentinel.Support.Converters;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace Sentinel.Extractors.Gui
{
    public class EditExtractor
        : IEditExtractorService
    {
        #region IEditExtractorService Members

        public void Edit(IExtractor extractor)
        {
            Debug.Assert(extractor != null, "Extractor must be supplied to allow editing.");

            var window = new AddEditExtractorWindow();
            var data = new AddEditExtractor(window, true);
            window.DataContext = data;
            window.Owner = Application.Current.MainWindow;

            data.Name = extractor.Name;            
            data.Field = extractor.Field;
            data.Pattern = extractor.Pattern;
            data.Mode = extractor.Mode;

            bool? dialogResult = window.ShowDialog();

            if (dialogResult != null && (bool)dialogResult)
            {
                extractor.Name = data.Name;
                extractor.Pattern = data.Pattern;
                extractor.Mode = data.Mode;               
                extractor.Field = data.Field;
            }
        }

        #endregion IEditExtractorService Members
    }
}