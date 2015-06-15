namespace Sentinel.Logs.Gui
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows.Controls;

    using Sentinel.Services;
    using Sentinel.Views.Interfaces;

    using WpfExtras;

    /// <summary>
    /// Interaction logic for ViewSelectionPage.xaml
    /// </summary>
    public partial class ViewSelectionPage : IWizardPage
    {
        private readonly ObservableCollection<IWizardPage> children = new ObservableCollection<IWizardPage>();

        private readonly ReadOnlyObservableCollection<IWizardPage> readonlyChildren;

        private bool horizontal;

        private bool multipleView;

        private int primaryIndex;

        private int secondaryIndex;

        private bool singleView = true;

        private bool vertical = true;

        private bool multipleViewsSupported = false;

        private IEnumerable<IViewInformation> registeredViews;

        public ViewSelectionPage()
        {
            InitializeComponent();
            DataContext = this;

            readonlyChildren = new ReadOnlyObservableCollection<IWizardPage>(children);

            IViewManager vm = ServiceLocator.Instance.Get<IViewManager>();
            if (vm != null)
            {
                registeredViews = new List<IViewInformation>(vm.GetRegistered());
                
#if DISABLE_MULTIPLE_VIEWS
                MultipleViewsSupported = registeredViews.Count() > 1;
#endif
                SecondaryIndex = registeredViews.Count() > 1 ? 1 : PrimaryIndex;
            }

            PropertyChanged += PropertyChangedHandler;
        }

        public bool Horizontal
        {
            get
            {
                return horizontal;
            }

            set
            {
                if (horizontal == value) return;
                horizontal = value;
                OnPropertyChanged("Horizontal");
            }
        }

        public bool Vertical
        {
            get
            {
                return vertical;
            }

            set
            {
                if (vertical != value)
                {
                    vertical = value;
                    OnPropertyChanged("Vertical");
                }
            }
        }

        public bool MultipleViewsSupported
        {
            get
            {
                return multipleViewsSupported;
            }

            private set
            {
                if (multipleViewsSupported != value)
                {
                    multipleViewsSupported = value;
                    OnPropertyChanged("MultipleViewsSupported");
                }
            }
        }

        public bool MultipleView
        {
            get
            {
                return multipleView;
            }

            set
            {
                if (multipleView != value)
                {
                    multipleView = value;
                    OnPropertyChanged("MultipleView");
                }
            }
        }

        public bool SingleView
        {
            get
            {
                return singleView;
            }

            set
            {
                if (singleView != value)
                {
                    singleView = value;
                    OnPropertyChanged("SingleView");
                }
            }
        }

        public IEnumerable<IViewInformation> RegisteredViews
        {
            get
            {
                return registeredViews;
            }

            private set
            {
                if (registeredViews != value)
                {
                    registeredViews = value;
                    OnPropertyChanged("RegisteredViews");
                }
            }
        }

        public int PrimaryIndex
        {
            get
            {
                return primaryIndex;
            }

            set
            {
                if (primaryIndex != value)
                {
                    primaryIndex = value;
                    OnPropertyChanged("PrimaryIndex");
                }
            }
        }

        public int SecondaryIndex
        {
            get
            {
                return secondaryIndex;
            }

            set
            {
                if (secondaryIndex != value)
                {
                    secondaryIndex = value;
                    OnPropertyChanged("SecondaryIndex");
                }
            }
        }

        public string Title
        {
            get
            {
                return "Visualising the Log";
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
                return "Select the desired views to visualise the logger and its providers.";
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

        public object Save(object saveData)
        {
            Debug.Assert(saveData != null, "Expecting a non-null instance of a class to save settings into");
            Debug.Assert(saveData is NewLoggerSettings, "Expecting save data structure to be a NewLoggerSettings");

            NewLoggerSettings settings = saveData as NewLoggerSettings;
            if (settings != null)
            {
                settings.Views.Clear();
                settings.Views.Add(registeredViews.ElementAt(PrimaryIndex).Identifier);
                if (MultipleView)
                {
                    settings.Views.Add(registeredViews.ElementAt(SecondaryIndex).Identifier);
                    settings.IsVertical = Vertical;
                }
            }

            return saveData;
        }

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Horizontal":
                    Vertical = !Horizontal;
                    break;
                case "Vertical":
                    Horizontal = !Vertical;
                    break;
                case "SingleView":
                    MultipleView = !SingleView;
                    break;
                case "MultipleView":
                    SingleView = !MultipleView;
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }
    }
}