namespace Sentinel.Filters.Gui
{
    using System.Diagnostics;
    using System.Windows;

    using Sentinel.Filters.Interfaces;

    public class EditFilter : IEditFilterService
    {
        public void Edit(IFilter filter)
        {
            Debug.Assert(filter != null, "Filter must be supplied to allow editing.");

            var window = new AddEditFilterWindow();
            var data = new AddEditFilter(window, true);
            window.DataContext = data;
            window.Owner = Application.Current.MainWindow;

            data.Name = filter.Name;
            data.Field = filter.Field;
            data.Pattern = filter.Pattern;
            data.Mode = filter.Mode;

            var dialogResult = window.ShowDialog();

            if (dialogResult != null && (bool)dialogResult)
            {
                filter.Name = data.Name;
                filter.Pattern = data.Pattern;
                filter.Mode = data.Mode;
                filter.Field = data.Field;
            }
        }
    }
}