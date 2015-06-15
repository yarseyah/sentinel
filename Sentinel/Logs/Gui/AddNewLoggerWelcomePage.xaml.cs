namespace Sentinel.Logs.Gui
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Controls;

    using WpfExtras;

    /// <summary>
    /// Interaction logic for AddNewLoggerPage.xaml
    /// </summary>
    public partial class AddNewLoggerWelcomePage : IWizardPage
    {
        private readonly ObservableCollection<IWizardPage> children = new ObservableCollection<IWizardPage>();
        private readonly ReadOnlyObservableCollection<IWizardPage> readonlyChildren;

        private string description;

        private string title;

        public AddNewLoggerWelcomePage()
        {
            InitializeComponent();
            DataContext = this;

            readonlyChildren = new ReadOnlyObservableCollection<IWizardPage>(children);

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
                    OnPropertyChanged("Title");
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
                    OnPropertyChanged("Description");
                }
            }
        }

        public bool IsValid
        {
            get
            {
                return true;
            }
        }

        public Control PageContent
        {
            get
            {
                return this;
            }
        }

        public void AddChild(IWizardPage newItem)
        {
            children.Add(newItem);
            OnPropertyChanged("Children");
        }

        public void RemoveChild(IWizardPage item)
        {
            children.Remove(item);
            OnPropertyChanged("Children");
        }

        public ReadOnlyObservableCollection<IWizardPage> Children
        {
            get
            {
                return readonlyChildren;
            }
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
