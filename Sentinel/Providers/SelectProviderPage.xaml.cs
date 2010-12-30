using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Sentinel.Providers.Interfaces;
using Sentinel.Services;
using WpfExtras;

namespace Sentinel.Providers
{
    /// <summary>
    /// Interaction logic for SelectProviderPage.xaml
    /// </summary>
    public partial class SelectProviderPage : IWizardPage
    {
        private readonly ObservableCollection<IWizardPage> children = new ObservableCollection<IWizardPage>();

        private readonly ReadOnlyObservableCollection<IWizardPage> readonlyChildren;

        private string name;

        private bool isValid;

        private List<IProviderInfo> providers = new List<IProviderInfo>();

        /// <summary>
        /// The additionalPages collection will maintain any child pages created
        /// based upon the providers selection.  The indexes will match that of
        /// the providers collection.  This is purely being done so that if a setting
        /// is made on the child page, the provider changed (by going back) and then
        /// reverted, the original settings will still be there!
        /// </summary>
        private List<IWizardPage> additionalPages = new List<IWizardPage>();

        private int selectedProvider = -1;

        private string selectedProviderDescription;

        private readonly IProviderManager providerManager;

        public SelectProviderPage()
        {
            InitializeComponent();
            DataContext = this;

            readonlyChildren = new ReadOnlyObservableCollection<IWizardPage>(children);

            // Register to self so that we can handler user interactions.
            PropertyChanged += PropertyChangedHandler;

            providerManager = ServiceLocator.Instance.Get<IProviderManager>();
            if (providerManager != null)
            {
                foreach (Guid guid in providerManager)
                {
                    providers.Add(providerManager.GetInformation(guid));

                    // If any additional page, we shall cache them with the
                    // same index, so make sure the collection matches providers.
                    additionalPages.Add(null);
                }
            }

            LoggerName = "Untitled";
        }

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedProvider")
            {
                int index = SelectedProvider;
                IsValid = index != -1 && !string.IsNullOrEmpty(name);
                SetChildPages(index);
                return;
            }
        }

        private void SetChildPages(int index)
        {
            if ( index < 0 || index >= providers.Count ) return;

            // See whether a page can be cached.
            if ( additionalPages[index] == null )
            {
                SelectedProviderDescription = providers[index].Description;
                IProviderInfo info = providers[index];

                if (providerManager != null)
                {
                    additionalPages[index] = providerManager.GetConfiguration<IWizardPage>(info.Identifier);
                }
            }

            while(children.Any())
            {
                IWizardPage p = children.First();
                RemoveChild(p);
            }

            if (additionalPages[index] != null)
            {
                AddChild(additionalPages[index]);
            }
        }

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
                if (selectedProvider == value) return;
                selectedProvider = value;
                OnPropertyChanged("SelectedProvider");
            }
        }

        public string SelectedProviderDescription
        {
            get
            {
                return selectedProviderDescription;
            }
            private set
            {
                if (selectedProviderDescription == value) return;
                selectedProviderDescription = value;
                OnPropertyChanged("SelectedProviderDescription");
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
                if (name == value) return;
                name = value;
                OnPropertyChanged("LoggerName");
            }
        }

        #region Implementation of IWizardPage

        public string Title
        {
            get
            {
                return "Select Provider";
            }
        }

        public string Description
        {
            get
            {
                return "Select a log provider from the registered providers";
            }
        }

        public ReadOnlyObservableCollection<IWizardPage> Children
        {
            get
            {
                return readonlyChildren;
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
        }

        public void RemoveChild(IWizardPage item)
        {
            children.Remove(item);
        }

        public object Save(object saveData)
        {
            if ( saveData == null || !(saveData is ProviderSettings) )
            {
                saveData = new ProviderSettings();
            }

            (saveData as ProviderSettings).Info = providers[SelectedProvider];
            (saveData as ProviderSettings).Name = name;

            return saveData;
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

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            if ( SelectedProvider == -1 && providers.Any() )
            {
                SelectedProvider = 0;
            }
        }
    }
}