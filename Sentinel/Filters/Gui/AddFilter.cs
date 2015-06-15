namespace Sentinel.Filters.Gui
{
    using System.Windows;

    using Sentinel.Filters.Interfaces;
    using Sentinel.Services;

    public class AddFilter
        : IAddFilterService
    {
        public void Add()
        {
            AddEditFilterWindow filterWindow = new AddEditFilterWindow();
            using (AddEditFilter data = new AddEditFilter(filterWindow, false))
            {
                filterWindow.DataContext = data;
                filterWindow.Owner = Application.Current.MainWindow;
                bool? dialogResult = filterWindow.ShowDialog();
                if (dialogResult != null && (bool)dialogResult)
                {
                    Filter filter = Construct(data);
                    if (filter != null)
                    {
                        var service = ServiceLocator.Instance.Get<IFilteringService<IFilter>>();
                        if (service != null)
                        {
                            service.Filters.Add(filter);
                        }
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
                Enabled = true
            };
        }
    }
}