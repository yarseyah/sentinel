namespace WpfExtras
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    ///   Interaction logic for Wizard.xaml.
    /// </summary>
    public partial class Wizard
    {
        public static readonly DependencyProperty CurrentPageContentProperty = DependencyProperty.Register(
            "CurrentPageContent",
            typeof(Control),
            typeof(Wizard),
            new UIPropertyMetadata(null));

        public static readonly DependencyProperty ShowNavigationTreeProperty = DependencyProperty.Register(
            "ShowNavigationTree",
            typeof(bool),
            typeof(Wizard),
            new UIPropertyMetadata(false));

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel",
            typeof(INotifyPropertyChanged),
            typeof(Wizard),
            new UIPropertyMetadata(null));

        private readonly PageNavigationTreeEntry navigationTree;

        private readonly RootPage root = new RootPage();

        private IWizardPage currentPage;

        private IWizardPage first;

        private IWizardPage last;

        private IWizardPage next;

        private IWizardPage previous;

        public Wizard()
        {
            InitializeComponent();

            navigationTree = new PageNavigationTreeEntry(root);

            Back = new DelegateCommand(e => GoBack(), q => CanGoBack);
            Forward = new DelegateCommand(e => GoForward(), q => CanGoForward);
            Finish = new DelegateCommand(e => AcceptDialog(), q => CanFinish);
            Cancel = new DelegateCommand(e => CancelDialog(), q => true);

            // Register to root page that we are interested in changes.
            root.PropertyChanged += PagesPropertyChangedHandler;

            DataContext = this;
        }

        public ICommand Back { get; private set; }

        public ICommand Cancel { get; private set; }

        public Control CurrentPageContent
        {
            get
            {
                return (Control)GetValue(CurrentPageContentProperty);
            }

            set
            {
                SetValue(CurrentPageContentProperty, value);
            }
        }

        public ICommand Finish { get; private set; }

        public ICommand Forward { get; private set; }

        public ReadOnlyObservableCollection<PageNavigationTreeEntry> NavigationTree => navigationTree.Children;

        public object SavedData { get; set; }

        public bool ShowNavigationTree
        {
            get
            {
                return (bool)GetValue(ShowNavigationTreeProperty);
            }

            set
            {
                SetValue(ShowNavigationTreeProperty, value);
            }
        }

        public INotifyPropertyChanged ViewModel
        {
            get
            {
                return (INotifyPropertyChanged)GetValue(ViewModelProperty);
            }

            set
            {
                SetValue(ViewModelProperty, value);
            }
        }

        private bool CanFinish => currentPage != null && currentPage.IsValid && currentPage == last;

        private bool CanGoBack => currentPage != null && previous != null;

        private bool CanGoForward => currentPage != null && currentPage.IsValid && next != null;

        public void AddPage(IWizardPage page)
        {
            root.AddChild(page);
        }

        private void AcceptDialog()
        {
            if (currentPage != null)
            {
                SavedData = currentPage.Save(SavedData);
            }

            DialogResult = true;
            Close();
        }

        private void CancelDialog()
        {
            DialogResult = false;
            Close();
        }

        private void ChildObserver(object sender, PropertyChangedEventArgs e)
        {
            Debug.Assert(currentPage != null, "Should not be reacting to child change if there is no current page!");

            switch (e.PropertyName)
            {
                case "Children":
                    UpdateNavigationPossibilities();
                    break;
                default:
                    break;
            }
        }

        private void GoBack()
        {
            Debug.Assert(CanGoBack, "Should not be able to attempt to go back, the button's state should be disabled.");

            SwitchPage(PageChange.Previous);
        }

        private void GoForward()
        {
            Debug.Assert(CanGoForward, "Should not be able to attempt to go forward, button should be disabled.");

            SwitchPage(PageChange.Next);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Debug.Assert(root != null, "The hidden root page should have been constructed.");
            Debug.Assert(root.Children != null, "There must be at least one registered page");
            Debug.Assert(root.Children.Any(), "There must be at least one registered page");

            SwitchPage(PageChange.First);
        }

        private void PagesPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Children")
            {
                UpdateNavigationPossibilities();
            }
        }

        private void SwitchPage(PageChange change)
        {
            IWizardPage newPage = null;

            // If a current page exists, unbind from it.
            if (currentPage != null)
            {
                currentPage.PropertyChanged -= ChildObserver;
                (currentPage.Children as INotifyCollectionChanged).CollectionChanged -= WizardPageCollectionChanged;

                switch (change)
                {
                    case PageChange.First:
                        newPage = first;
                        break;
                    case PageChange.Previous:
                        newPage = previous;
                        break;
                    case PageChange.Next:
                        SavedData = currentPage.Save(SavedData);
                        newPage = next;
                        break;
                    case PageChange.Last:
                        SavedData = currentPage.Save(SavedData);
                        newPage = last;
                        break;
                }
            }
            else
            {
                if (change == PageChange.Next || change == PageChange.Previous)
                {
                    throw new NotSupportedException("Unable to go to previous/next because no current page defined.");
                }

                if (change == PageChange.First && first == null)
                {
                    throw new NotSupportedException("Unable to go to first page because no pages are registered.");
                }

                if (change == PageChange.Last && last == null)
                {
                    throw new NotSupportedException("Unable to go to last page because no pages are registered.");
                }

                newPage = change == PageChange.First ? first : last;
            }

            Debug.Assert(newPage != null, "New page was a null reference, should not be the case!");

            // Register interest in changes within child view.
            newPage.PropertyChanged += ChildObserver;
            (newPage.Children as INotifyCollectionChanged).CollectionChanged += WizardPageCollectionChanged;

            // Update current page.
            CurrentPageContent = newPage.PageContent;
            currentPage = newPage;
            navigationTree.SetCurrentPage(currentPage);

            UpdateNavigationPossibilities();
        }

        private void UpdateNavigationPossibilities()
        {
            var navPossibilities = new EstablishPossibleNavigation(root, currentPage);
            navPossibilities.Execute();

            first = navPossibilities.First;
            last = navPossibilities.Last;
            next = navPossibilities.Next;
            previous = navPossibilities.Previous;
        }

        private void WizardPageCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateNavigationPossibilities();
        }

        private sealed class RootPage : IWizardPage
        {
            private readonly ObservableCollection<IWizardPage> pages = new ObservableCollection<IWizardPage>();

            public RootPage()
            {
                Children = new ReadOnlyObservableCollection<IWizardPage>(pages);
            }

            public event PropertyChangedEventHandler PropertyChanged;

            public ReadOnlyObservableCollection<IWizardPage> Children { get; }

            public string Description => "Hidden Root Page - this page should never be shown";

            public bool IsValid => false;

            public Control PageContent
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public string Title => "Hidden Root Page";

            public void AddChild(IWizardPage newItem)
            {
                Debug.Assert(pages != null, "Pages collection has not been constructed");
                pages.Add(newItem);
                OnPropertyChanged(nameof(Children));
            }

            public void RemoveChild(IWizardPage item)
            {
                pages.Remove(item);
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
}