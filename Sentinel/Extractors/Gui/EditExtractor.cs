namespace Sentinel.Extractors.Gui
{
    using System.Diagnostics;
    using System.Windows;

    using Sentinel.Extractors.Interfaces;

    public class EditExtractor : IEditExtractorService
    {
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

            var dialogResult = window.ShowDialog();

            if (dialogResult != null && (bool)dialogResult)
            {
                extractor.Name = data.Name;
                extractor.Pattern = data.Pattern;
                extractor.Mode = data.Mode;
                extractor.Field = data.Field;
            }
        }
    }
}