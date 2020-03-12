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
    /// Interaction logic for ViewSelectionPage.xaml.
    /// </summary>
    public partial class ViewSelectionPage : IWizardPage
    {
        private readonly ObservableCollection<IWizardPage> children = new ObservableCollection<IWizardPage>();

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

            Children = new ReadOnlyObservableCollection<IWizardPage>(children);

            IViewManager vm = ServiceLocator.Instance.Get<IViewManager>();
            if (vm != null)
            {
                registeredViews = new List<IViewInformation>(vm.Registered);

#if DISABLE_MULTIPLE_VIEWS
                MultipleViewsSupported = registeredViews.Count() > 1;
#endif
                SecondaryIndex = registeredViews.Count() > 1 ? 1 : PrimaryIndex;
            }

            PropertyChanged += PropertyChangedHandler;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool Horizontal
        {
            get
            {
                return horizontal;
            }

            set
            {
                if (horizontal == value)
                {
                    return;
                }

                horizontal = value;
                OnPropertyChanged(nameof(Horizontal));
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
                    OnPropertyChanged(nameof(Vertical));
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
                    OnPropertyChanged(nameof(MultipleViewsSupported));
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
                    OnPropertyChanged(nameof(MultipleView));
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
                    OnPropertyChanged(nameof(SingleView));
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
                if (!Equals(registeredViews, value))
                {
                    registeredViews = value;
                    OnPropertyChanged(nameof(RegisteredViews));
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
                    OnPropertyChanged(nameof(PrimaryIndex));
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
                    OnPropertyChanged(nameof(SecondaryIndex));
                }
            }
        }

        public string Title => "Visualising the Log";

        public ReadOnlyObservableCollection<IWizardPage> Children { get; }

        public string Description => "Select the desired views to visualise the logger and its providers.";

        public bool IsValid => true;

        public Control PageContent => this;

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
            Debug.Assert(saveData != null, "Expecting a non-null instance of a class to save settings into");
            Debug.Assert(saveData is NewLoggerSettings, "Expecting save data structure to be a NewLoggerSettings");

            var settings = saveData as NewLoggerSettings;
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

        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
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
    }
}