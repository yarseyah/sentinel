namespace Sentinel.Filters.Gui
{
    using System.Windows.Controls;

    using Sentinel.Filters.Interfaces;
    using Sentinel.Services;

    /// <summary>
    /// Interaction logic for FiltersControl.xaml.
    /// </summary>
    public partial class FiltersControl : UserControl
    {
        public FiltersControl()
        {
            InitializeComponent();

            var service = ServiceLocator.Instance.Get<IFilteringService<IFilter>>();
            if (service != null)
            {
                Filters = service;
            }

            DataContext = this;
        }

        public IFilteringService<IFilter> Filters { get; private set; }
    }
}