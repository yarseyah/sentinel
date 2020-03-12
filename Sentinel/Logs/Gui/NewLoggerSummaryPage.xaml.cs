namespace Sentinel.Logs.Gui
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Controls;

    using WpfExtras;

    /// <summary>
    /// Interaction logic for NewLoggerSummaryPage.xaml.
    /// </summary>
    public partial class NewLoggerSummaryPage : IWizardPage
    {
        private readonly ObservableCollection<IWizardPage> children = new ObservableCollection<IWizardPage>();

        public NewLoggerSummaryPage()
        {
            InitializeComponent();

            Children = new ReadOnlyObservableCollection<IWizardPage>(children);

            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Title => "Summary";

        public string Description => "Review the selections made in this Wizard.";

        public bool IsValid => true;

        public ReadOnlyObservableCollection<IWizardPage> Children { get; }

        public Control PageContent => this;

        public void AddChild(IWizardPage newItem)
        {
            children.Add(newItem);
        }

        public void RemoveChild(IWizardPage item)
        {
            children.Remove(item);
        }

        public object Save(object saveData)
        {
            return saveData;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }
    }
}