namespace Sentinel.Classification.Gui
{
    using System.Windows.Controls;

    using Sentinel.Classification.Interfaces;
    using Sentinel.Services;

    /// <summary>
    /// Interaction logic for ClassificationsControl.xaml.
    /// </summary>
    public partial class ClassificationsControl : UserControl
    {
        public ClassificationsControl()
        {
            InitializeComponent();
            Classifier = ServiceLocator.Instance.Get<IClassifyingService<IClassifier>>();
            DataContext = this;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public IClassifyingService<IClassifier> Classifier { get; private set; }
    }
}