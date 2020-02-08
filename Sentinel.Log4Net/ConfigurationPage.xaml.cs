namespace Sentinel.Log4Net
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows.Controls;

    using Sentinel.Interfaces.Providers;

    using WpfExtras;

    /// <summary>
    /// Interaction logic for ConfigurationPage.xaml.
    /// </summary>
    public partial class ConfigurationPage : IWizardPage
    {
        private readonly ObservableCollection<IWizardPage> children = new ObservableCollection<IWizardPage>();

        private readonly ReadOnlyObservableCollection<IWizardPage> readonlyChildren;

        private bool isValid;

        private int port;

        public ConfigurationPage()
        {
            InitializeComponent();
            DataContext = this;

            readonlyChildren = new ReadOnlyObservableCollection<IWizardPage>(children);

            // Register to self so that we can handler user interactions.
            PropertyChanged += SelectProviderPagePropertyChanged;

            Port = 9998;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int Port
        {
            get
            {
                return port;
            }

            set
            {
                if (port != value)
                {
                    port = value;
                    OnPropertyChanged(nameof(Port));
                }
            }
        }

        public string Title
        {
            get
            {
                return "Configure Provider";
            }
        }

        public ReadOnlyObservableCollection<IWizardPage> Children
        {
            get
            {
                return readonlyChildren;
            }
        }

        public string Description
        {
            get
            {
                return "Network settings to be used by new provider";
            }
        }

        public bool IsValid
        {
            get
            {
                return isValid;
            }

            private set
            {
                if (isValid != value)
                {
                    isValid = value;
                    OnPropertyChanged(nameof(IsValid));
                }
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
            OnPropertyChanged(nameof(Children));
        }

        public void RemoveChild(IWizardPage item)
        {
            children.Remove(item);
            OnPropertyChanged(nameof(Children));
        }

        public object Save(object saveData)
        {
            Debug.Assert(saveData != null, "Expecting the save-data component to have details from the previous pages.");
            Debug.Assert(
                saveData is IProviderSettings,
                "Expecting the save-data component to be of an IProviderSettings type.");

            var providerInfo = (IProviderSettings)saveData;
            return new UdpAppenderSettings(providerInfo) { Port = Port };
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

        private void SelectProviderPagePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Port")
            {
                var state = port > 2000;
                Trace.WriteLine(string.Format("Setting PageValidates to {0}", state));
                IsValid = state;
            }
        }
    }
}