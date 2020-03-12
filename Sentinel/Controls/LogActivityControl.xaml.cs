namespace Sentinel.Controls
{
    using Sentinel.Highlighters.Interfaces;
    using Sentinel.Services;

    /// <summary>
    /// Interaction logic for LogActivityControl.xaml.
    /// </summary>
    public partial class LogActivityControl
    {
        public LogActivityControl()
        {
            InitializeComponent();
            Highlight = ServiceLocator.Instance.Get<IHighlightingService<IHighlighter>>();
        }

        public IHighlightingService<IHighlighter> Highlight { get; private set; }
    }
}