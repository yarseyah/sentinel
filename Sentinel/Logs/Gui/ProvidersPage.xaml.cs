
namespace Sentinel.Logs.Gui
{
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using Sentinel.Interfaces.Providers;
    using Sentinel.Providers;
    using Sentinel.Providers.Interfaces;
    using Sentinel.Services;

    using WpfExtras;

    using DelegateCommand = Sentinel.Support.Mvvm.DelegateCommand;

    /// <summary>
    /// Interaction logic for ProvidersPage.xaml
    /// </summary>
    public partial class ProvidersPage : IWizardPage, IDataErrorInfo
    {
        private readonly ObservableCollection<IWizardPage> children = new ObservableCollection<IWizardPage>();

        private readonly ReadOnlyObservableCollection<IWizardPage> readonlyChildren;

        private int selectedProviderIndex = -1;
        private bool isValid;

        public ICommand Add { get; private set; }

        public ICommand Remove { get; private set; }

        public ObservableCollection<PendingProviderRecord> Providers { get; private set; }

        public ProvidersPage()
        {
            InitializeComponent();
            DataContext = this;

            Providers = new ObservableCollection<PendingProviderRecord>();

            readonlyChildren = new ReadOnlyObservableCollection<IWizardPage>(children);

            Add = new DelegateCommand(AddNewProvider);
            Remove = new DelegateCommand(RemoveSelectedProvider, e => SelectedProviderIndex != -1);

            Providers.CollectionChanged += ProvidersCollectionChanged;
        }

        private void ProvidersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("Providers");
            IsValid = Error == null;
        }

        public int SelectedProviderIndex
        {
            get
            {
                return selectedProviderIndex;
            }
            set
            {
                if (selectedProviderIndex == value) return;
                selectedProviderIndex = value;
                OnPropertyChanged("SelectedProviderIndex");
            }
        }

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

        #region Implementation of IWizardPage

        public string Title
        {
            get
            {
                return "Provider Registration";
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
                return "Specify the source-providers for the new logger.";
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

        public Control PageContent { get { return this; } }

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
            Debug.Assert(saveData != null, "Expecting an instance to save data into");
            Debug.Assert(saveData is NewLoggerSettings, "Expecting a NewLoggerSettings instance");

            NewLoggerSettings settings = saveData as NewLoggerSettings;
            if (settings != null)
            {
                settings.Providers.Clear();
                foreach (var provider in Providers)
                {
                    settings.Providers.Add(provider);
                }
            }

            return saveData;
        }

        #endregion

        private void AddNewProvider(object obj)
        {
            ServiceLocator services = ServiceLocator.Instance;

            INewProviderWizard wizard = services.Get<INewProviderWizard>();

            if (wizard != null)
            {
                if (wizard.Display((Window) this.Parent))
                {
                    IProviderInfo info = wizard.Provider;
                    IProviderSettings settings = wizard.Settings;

                    PendingProviderRecord rec = new PendingProviderRecord
                                                    {
                                                        Info = info,
                                                        Settings = settings
                                                    };

                    Providers.Add(rec);
                }
            }
            else
            {
                Trace.WriteLine("Should have a wizard registered for enrolling new providers.");
            }
        }

        private void RemoveSelectedProvider(object obj)
        {
            // TODO: confirmation of deletion.

            int index = SelectedProviderIndex;
            if ( index != -1 && index < Providers.Count )
            {
                Providers.RemoveAt(index);
            }
            else
            {
                // TODO: error condition.
            }
        }

        #region Implementation of IDataErrorInfo

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <returns>
        /// The error message for the property. The default is an empty string ("").
        /// </returns>
        /// <param name="columnName">The name of the property whose error message to get. 
        ///                 </param>
        public string this[string columnName]
        {
            get
            {
                switch ( columnName )
                {
                    case "Providers":
                        return ValidateProviders();
                }

                return null;
            }
        }

        /// <summary>
        /// Gets an error message indicating what is wrong with this object.
        /// </summary>
        /// <returns>
        /// An error message indicating what is wrong with this object. The default is an empty string ("").
        /// </returns>
        public string Error
        {
            get
            {
                return this["Providers"];
            }
        }

        #endregion

        private string ValidateProviders()
        {
            if ( Providers == null )
            {
                return "[Internal Error] Providers structure can not be null";
            }

            if ( Providers.Count == 0 )
            {
                return "At least one provider must be specified";
            }

            if ( Providers.GroupBy( p => p.Settings.Name ).Where( g => g.Count() > 1).Any() )
            {
                return "Duplicate provider names are not supported, please provide appropriate names.";
            }

            var providersWithPorts = Providers
                .Select(p => p.Settings)
                .OfType<NetworkSettings>();
            var providersGroupedByPort = providersWithPorts
                .GroupBy(p => p.Port);

            if ( providersGroupedByPort.Where(g => g.Count() > 1).Any() )
            {
                // Duplicate port #
                int duplicatePort = providersGroupedByPort
                    .Where( g => g.Count() > 1 )
                    .First().Key;

                return
                    string.Format(
                        "Duplicate network ports are not permitted, you have specified port number {0} more than once",
                        duplicatePort);
            }

            IProviderManager providerManager = ServiceLocator.Instance.Get<IProviderManager>();

            if ( providerManager != null 
                && Providers.Select(p => p.Settings.Name).Intersect(providerManager.GetInstances().Select(p2 => p2.Name)).Any() )
            {
                var duplicates = Providers
                    .Select(p => p.Settings.Name)
                    .Intersect(providerManager.GetInstances().Select(p2 => p2.Name));
                return string.Format(
                    "Providers should be uniquely named, there is already one called {0}",
                    duplicates.First());
            }

            if ( providerManager != null 
                && providerManager.GetInstances().OfType<INetworkProvider>().Any()
                && providersWithPorts.Any())
            {
                // Get the two lists:
                var instances = providerManager
                    .GetInstances()
                    .OfType<INetworkProvider>()
                    .Select(i => i.Port);
                var newSettings = providersWithPorts
                    .Select(p => p.Port);

                var intersection = instances.Intersect(newSettings);

                if ( intersection.Any() )
                {
                    return string.Format(
                        "Network port numbers must be unique, port number {0} is already in use.",
                        intersection.First());
                }
            }

            return null;
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            OnPropertyChanged("Providers");
        }
    }
}
