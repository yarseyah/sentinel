namespace Sentinel.Filters.Gui
{
    using System.Windows;

    using Sentinel.Filters.Interfaces;
    using Sentinel.Services;

    public class AddFilter : IAddFilterService
    {
        public void Add()
        {
            var filterWindow = new AddEditFilterWindow();
            using (var data = new AddEditFilter(filterWindow, false))
            {
                filterWindow.DataContext = data;
                filterWindow.Owner = Application.Current.MainWindow;
                var dialogResult = filterWindow.ShowDialog();
                if (dialogResult != null && (bool)dialogResult)
                {
                    var filter = Construct(data);
                    if (filter != null)
                    {
                        var service = ServiceLocator.Instance.Get<IFilteringService<IFilter>>();
                        service?.Filters.Add(filter);
                    }
                }
            }
        }

        private static Filter Construct(AddEditFilter data)
        {
            return new Filter
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