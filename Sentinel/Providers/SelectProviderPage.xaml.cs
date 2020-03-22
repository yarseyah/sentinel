namespace Sentinel.Providers
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    using Common.Logging;

    using Sentinel.Interfaces.Providers;
    using Sentinel.Providers.Interfaces;
    using Sentinel.Services;

    using WpfExtras;

    /// <summary>
    /// Interaction logic for SelectProviderPage.xaml.
    /// </summary>
    public partial class SelectProviderPage : IWizardPage
    {
        private static readonly ILog Log = LogManager.GetLogger<SelectProviderPage>();

        private readonly ObservableCollection<IWizardPage> children = new ObservableCollection<IWizardPage>();

        private readonly List<IProviderInfo> providers = new List<IProviderInfo>();

        private readonly IProviderManager providerManager;

        /// <summary>
        /// The additionalPages collection will maintain any child pages created
        /// based upon the providers selection.  The indexes will match that of
        /// the providers collection.  This is purely being done so that if a setting
        /// is made on the child page, the provider changed (by going back) and then
        /// reverted, the original settings will still be there.
        /// </summary>
        private readonly List<IWizardPage> additionalPages = new List<IWizardPage>();

        private string name;

        private bool isValid;

        private int selectedProvider = -1;

        private string selectedProviderDescription;

        public SelectProviderPage()
        {
            InitializeComponent();
            DataContext = this;

            Children = new ReadOnlyObservableCollection<IWizardPage>(children);

            // Register to self so that we can handler user interactions.
            PropertyChanged += PropertyChangedHandler;

            providerManager = ServiceLocator.Instance.Get<IProviderManager>();
            if (providerManager != null)
            {
                foreach (var guid in providerManager)
                {
                    providers.Add(providerManager.GetInformation(guid));

                    // If any additional page, we shall cache them with the
                    // same index, so make sure the collection matches providers.
                    additionalPages.Add(null);
                }
            }

            LoggerName = "Untitled";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public IEnumerable<string> Providers
        {
            get
            {
                return providers.Select(i => i.Name);
            }
        }

        public int SelectedProvider
        {
            get
            {
                return selectedProvider;
            }

            set
            {
                if (selectedProvider != value)
                {
                    Log.DebugFormat("Selected provider index changed to {0}", value);

                    selectedProvider = value;
                    OnPropertyChanged(nameof(SelectedProvider));
                }
            }
        }

        public string Title
        {
            get
            {
                return "Select Provider";
            }
        }

        public string LoggerName
        {
            get
            {
                return name;
            }

            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged(nameof(LoggerName));
                }
            }
        }

        public string Description => "Select a log provider from the registered providers";

        public ReadOnlyObservableCollection<IWizardPage> Children { get; }

        public string SelectedProviderDescription
        {
            get
            {
                Log.DebugFormat("Retrieving description: {0}", selectedProviderDescription);
                return selectedProviderDescription;
            }

            private set
            {
                if (selectedProviderDescription != value)
                {
                    selectedProviderDescription = value;
                    OnPropertyChanged(nameof(SelectedProviderDescription));
                }
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
            if (!(saveData is ProviderSettings))
            {
                saveData = new ProviderSettings();
            }

            ((ProviderSettings)saveData).Info = providers[SelectedProvider];
            ((ProviderSettings)saveData).Name = name;

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

        private void SetChildPages(int index)
        {
            if (index < 0 || index >= providers.Count)
            {
                return;
            }

            SelectedProviderDescription = providers[index].Description;

            // See whether a page can be cached.
            if (additionalPages[index] == null)
            {
                var info = providers[index];

                if (providerManager != null)
                {
                    additionalPages[index] = providerManager.GetConfiguration<IWizardPage>(info.Identifier);
                }
            }

            while (children.Any())
            {
                var p = children.First();
                RemoveChild(p);
            }

            if (additionalPages[index] != null)
            {
                AddChild(additionalPages[index]);
            }
        }

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedProvider")
            {
                var index = SelectedProvider;
                IsValid = index != -1 && !string.IsNullOrEmpty(name);
                SetChildPages(index);
            }
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            if (SelectedProvider == -1 && providers.Any())
            {
                SelectedProvider = 0;
            }
        }
    }
}