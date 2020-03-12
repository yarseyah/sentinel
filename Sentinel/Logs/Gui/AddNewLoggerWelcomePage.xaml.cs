namespace Sentinel.Logs.Gui
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Controls;

    using WpfExtras;

    /// <summary>
    /// Interaction logic for AddNewLoggerPage.xaml.
    /// </summary>
    public partial class AddNewLoggerWelcomePage : IWizardPage
    {
        private readonly ObservableCollection<IWizardPage> children = new ObservableCollection<IWizardPage>();

        private string description;

        private string title;

        public AddNewLoggerWelcomePage()
        {
            InitializeComponent();
            DataContext = this;

            Children = new ReadOnlyObservableCollection<IWizardPage>(children);

            Title = "Sentinel Logs";
            Description = "Information about how Sentinel works with loggers, providers and views";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Title
        {
            get
            {
                return title;
            }

            private set
            {
                if (title != value)
                {
                    title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        public string Description
        {
            get
            {
                return description;
            }

            private set
            {
                if (description != value)
                {
                    description = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }

        public bool IsValid => true;

        public Control PageContent => this;

        public ReadOnlyObservableCollection<IWizardPage> Children { get; }

        public void AddChild(IWizardPage newItem)
        {
            children.Add(newItem);
            OnPropertyChanged(nameof(Children));
        }

        public void RemoveChild(IWizardPage item)
        {
            children.Remove(item);
            OnPropertyChanged(nameof(Children));
        }

        public object Save(object saveData)
        {
            return saveData;
        }

        /// <summary>
        ///   Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name = "propertyName">The property that has a new value.</param>
        private void OnPropertyChanged(string propertyName)
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
