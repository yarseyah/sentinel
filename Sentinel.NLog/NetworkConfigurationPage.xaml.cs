namespace Sentinel.NLog
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows.Controls;

    using Sentinel.Interfaces.Providers;

    using WpfExtras;

    /// <summary>
    /// Interaction logic for NetworkConfigurationPage.xaml
    /// </summary>
    public partial class NetworkConfigurationPage : IWizardPage
    {
        private readonly ObservableCollection<IWizardPage> children = new ObservableCollection<IWizardPage>();

        private readonly ReadOnlyObservableCollection<IWizardPage> readonlyChildren;

        private bool isValid;

        private int port;

        private bool isUdp = true;

        public virtual bool SupportsTcp { get { return true; } }

        public virtual bool SupportsUdp { get { return true; } }

        public NetworkConfigurationPage()
        {
            InitializeComponent();
            DataContext = this;

            readonlyChildren = new ReadOnlyObservableCollection<IWizardPage>(children);

            // Register to self so that we can handler user interactions.
            PropertyChanged += SelectProviderPage_PropertyChanged;

            Port = 9999;
        }

        private void SelectProviderPage_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Port")
            {
                bool state = port > 2000;
                Trace.WriteLine(String.Format("Setting PageValidates to {0}", state));
                IsValid = state;
            }
        }

        public int Port
        {
            get
            {
                return port;
            }
            set
            {
                if (port == value) return;
                port = value;
                OnPropertyChanged("Port");
            }
        }
					
        public bool IsUdp
        {
            get
            {
                return isUdp;
            }
            set
            {
                if (isUdp == value) return;
                isUdp = value;
                OnPropertyChanged("IsUdp");
            }
        }

        #region Implementation of IWizardPage

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
                if (isValid == value) return;
                isValid = value;
                OnPropertyChanged("IsValid");
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

        public object Save(object saveData)
        {
            Debug.Assert(saveData != null, "Expecting the save-data component to have details from the previous pages.");
            Debug.Assert(saveData is IProviderSettings, "Expecting the save-data component to be of an IProviderSettings type.");

            var previousInfo = (IProviderSettings)saveData;

            return new NetworkSettings
                       {
                           Name = previousInfo.Name,
                           Info = previousInfo.Info,
                           Port = Port,
                           Protocol = IsUdp ? NetworkProtocol.Udp : NetworkProtocol.Udp
                       };
        }


        #endregion

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion

        private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // Establish default selection
            Debug.Assert(SupportsUdp || SupportsTcp, "The provider needs to support at least one of UDP or TCP");
            IsUdp = SupportsUdp;
        }

    }
}
