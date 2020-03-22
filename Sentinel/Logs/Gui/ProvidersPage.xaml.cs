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

    using Common.Logging;

    using Sentinel.Interfaces.Providers;
    using Sentinel.NLog;
    using Sentinel.Providers.Interfaces;
    using Sentinel.Services;

    using WpfExtras;

    /// <summary>
    /// Interaction logic for ProvidersPage.xaml.
    /// </summary>
    public partial class ProvidersPage : IWizardPage, IDataErrorInfo
    {
        private static readonly ILog Log = LogManager.GetLogger<ProvidersPage>();

        private readonly ObservableCollection<IWizardPage> children = new ObservableCollection<IWizardPage>();

        private int selectedProviderIndex = -1;

        private bool isValid;

        public ProvidersPage()
        {
            InitializeComponent();
            DataContext = this;

            Providers = new ObservableCollection<PendingProviderRecord>();

            Children = new ReadOnlyObservableCollection<IWizardPage>(children);

            Add = new DelegateCommand(AddNewProvider);
            Remove = new DelegateCommand(RemoveSelectedProvider, e => SelectedProviderIndex != -1);

            Providers.CollectionChanged += ProvidersCollectionChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand Add { get; private set; }

        public ICommand Remove { get; private set; }

        public ObservableCollection<PendingProviderRecord> Providers { get; private set; }

        public int SelectedProviderIndex
        {
            get
            {
                return selectedProviderIndex;
            }

            set
            {
                if (selectedProviderIndex != value)
                {
                    Log.DebugFormat("Selected provider index is {0}", value);
                    selectedProviderIndex = value;
                    OnPropertyChanged(nameof(SelectedProviderIndex));
                }
            }
        }

        public string Title => "Provider Registration";

        public ReadOnlyObservableCollection<IWizardPage> Children { get; }

        public string Description => "Specify the source-providers for the new logger.";

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

        public Control PageContent => this;

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

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <returns>
        /// The error message for the property. The default is an empty string ("").
        /// </returns>
        /// <param name="columnName">The name of the property whose error message to get.</param>
        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case "Providers":
                        return ValidateProviders();
                }

                return null;
            }
        }

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

            var settings = saveData as NewLoggerSettings;
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

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        private void AddNewProvider(object obj)
        {
            var services = ServiceLocator.Instance;
            var wizard = services.Get<INewProviderWizard>();

            if (wizard != null)
            {
                if (wizard.Display((Window)Parent))
                {
                    var info = wizard.Provider;
                    var settings = wizard.Settings;

                    var rec = new PendingProviderRecord { Info = info, Settings = settings };

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
            var index = SelectedProviderIndex;
            if (index != -1 && index < Providers.Count)
            {
                Providers.RemoveAt(index);
            }
            else
            {
                // TODO: error condition.
            }
        }

        private string ValidateProviders()
        {
            if (Providers == null)
            {
                return "[Internal Error] Providers structure can not be null";
            }

            if (Providers.Count == 0)
            {
                return "At least one provider must be specified";
            }

            if (Providers.GroupBy(p => p.Settings.Name).Any(g => g.Count() > 1))
            {
                return "Duplicate provider names are not supported, please provide appropriate names.";
            }

            var providersWithPorts = Providers.Select(p => p.Settings).OfType<NetworkSettings>().ToList();
            var providersGroupedByPort = providersWithPorts
                .GroupBy(p => p.Port).ToList();

            if (providersGroupedByPort.Any(g => g.Count() > 1))
            {
                // Duplicate port #
                var duplicatePort = providersGroupedByPort.First(g => g.Count() > 1).Key;
                return
                    $"Duplicate network ports are not permitted, you have specified port number {duplicatePort} more than once";
            }

            var providerManager = ServiceLocator.Instance.Get<IProviderManager>();

            if (providerManager != null
                && Providers.Select(p => p.Settings.Name).Intersect(providerManager.Instances.Select(p2 => p2.Name)).Any())
            {
                var duplicates = Providers.Select(p => p.Settings.Name).Intersect(providerManager.Instances.Select(p2 => p2.Name));
                return $"Providers should be uniquely named, there is already one called {duplicates.First()}";
            }

            if (providerManager != null
                && providerManager.Instances.OfType<INetworkProvider>().Any()
                && providersWithPorts.Any())
            {
                // Get the two lists:
                var instances = providerManager.Instances.OfType<INetworkProvider>().Select(i => i.Port);
                var newSettings = providersWithPorts.Select(p => p.Port);

                var intersection = instances.Intersect(newSettings).ToList();
                if (intersection.Any())
                {
                    return $"Network port numbers must be unique, port number {intersection.First()} is already in use.";
                }
            }

            return null;
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            OnPropertyChanged(nameof(Providers));
        }

        private void ProvidersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Providers));
            IsValid = Error == null;
        }
    }
}
