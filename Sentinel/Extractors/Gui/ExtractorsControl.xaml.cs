namespace Sentinel.Extractors.Gui
{
    using System.Windows.Controls;

    using Sentinel.Extractors.Interfaces;
    using Sentinel.Services;

    /// <summary>
    /// Interaction logic for ExtractorsControl.xaml.
    /// </summary>
    public partial class ExtractorsControl : UserControl
    {
        public ExtractorsControl()
        {
            InitializeComponent();
            var service = ServiceLocator.Instance.Get<IExtractingService<IExtractor>>();
            if (service != null)
            {
                Extractors = service;
            }

            DataContext = this;
        }

        public IExtractingService<IExtractor> Extractors { get; private set; }
    }
}