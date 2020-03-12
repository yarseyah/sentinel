namespace Sentinel.Views.Gui
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using Sentinel.Interfaces;
    using Sentinel.Services;
    using Sentinel.Views.Interfaces;

    /// <summary>
    /// Interaction logic for MultipleViewFrame.xaml.
    /// </summary>
    public partial class MultipleViewFrame : INotifyPropertyChanged, IWindowFrame
    {
        private readonly IUserPreferences preferences = ServiceLocator.Instance.Get<IUserPreferences>();

        // private readonly ISearchHighlighter searchHighlighter;
        private readonly IViewManager viewManager = ServiceLocator.Instance.Get<IViewManager>();
        private ILogger log;
        private string primaryTitle;
        private ILogViewer primaryView;
        private string secondaryTitle;
        private ILogViewer secondaryView;
        private bool collapseSecondaryView;

        public MultipleViewFrame()
        {
            InitializeComponent();

            if (preferences is INotifyPropertyChanged)
            {
                (preferences as INotifyPropertyChanged).PropertyChanged += PreferencesChanged;
            }

            // Filters = ServiceLocator.Instance.Get<IFilteringService<IFilter>>();
            // searchHighlighter = ServiceLocator.Instance.Get<ISearchHighlighter>();
            SetupSplitter();

            DataContext = this;
        }

        ~MultipleViewFrame()
        {
            var changed = preferences as INotifyPropertyChanged;
            if (changed != null)
            {
                changed.PropertyChanged -= PreferencesChanged;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand Clear { get; private set; }

        public ICommand ClearActivity { get; private set; }

        public ICommand Save { get; private set; }

        public ILogViewer PrimaryView
        {
            get
            {
                return primaryView;
            }

            set
            {
                if (primaryView != value)
                {
                    primaryView = value;
                    OnPropertyChanged(nameof(PrimaryView));
                }
            }
        }

        public string PrimaryTitle
        {
            get
            {
                return primaryTitle;
            }

            set
            {
                if (primaryTitle != value)
                {
                    primaryTitle = value;
                    OnPropertyChanged(nameof(PrimaryTitle));
                }
            }
        }

        public string SecondaryTitle
        {
            get
            {
                return secondaryTitle;
            }

            set
            {
                if (secondaryTitle != value)
                {
                    secondaryTitle = value;
                    OnPropertyChanged(nameof(SecondaryTitle));
                }
            }
        }

        public ILogViewer SecondaryView
        {
            get
            {
                return secondaryView;
            }

            set
            {
                if (secondaryView != value)
                {
                    secondaryView = value;
                    OnPropertyChanged(nameof(SecondaryView));
                }
            }
        }

        public IUserPreferences Preferences => preferences;

        public ILogger Log
        {
            get
            {
                return log;
            }

            set
            {
                if (log != value)
                {
                    log = value;
                    OnPropertyChanged(nameof(Log));
                }
            }
        }

        public void SetViews(IEnumerable<string> viewIdentifiers)
        {
            var identifiers = viewIdentifiers as string[] ?? viewIdentifiers.ToArray();
            if (identifiers.Any())
            {
                var guid = identifiers.ElementAt(0);
                PrimaryView = viewManager.GetInstance(guid);
                PrimaryView.SetLogger(log);
                PrimaryTitle = viewManager.Get(guid).Name;
            }

            if (identifiers.Length >= 2)
            {
                var guid = identifiers.ElementAt(1);
                SecondaryView = viewManager.GetInstance(guid);
                SecondaryView.SetLogger(log);
                SecondaryTitle = viewManager.Get(guid).Name;
            }

            if (identifiers.Length == 1)
            {
                CollapseSecondaryView();
            }
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

        private void PreferencesChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "UseStackedLayout")
            {
                SetupSplitter();
            }
        }

        private void SetupSplitter()
        {
            if (!collapseSecondaryView)
            {
                var vertical = preferences.UseStackedLayout;

                var rowSpan = vertical ? 1 : 3;
                var colSpan = vertical ? 3 : 1;

                Grid.SetRowSpan(first, rowSpan);
                Grid.SetRowSpan(splitter, rowSpan);
                Grid.SetRowSpan(second, rowSpan);

                Grid.SetColumnSpan(first, colSpan);
                Grid.SetColumnSpan(splitter, colSpan);
                Grid.SetColumnSpan(second, colSpan);

                splitter.Width = vertical ? int.MaxValue : 5;
                splitter.Height = vertical ? 5 : int.MaxValue;
                splitter.HorizontalAlignment = vertical
                                                   ? HorizontalAlignment.Stretch
                                                   : HorizontalAlignment.Left;
                splitter.VerticalAlignment = vertical
                                                 ? VerticalAlignment.Top
                                                 : VerticalAlignment.Stretch;

                Grid.SetColumn(first, 0);
                Grid.SetRow(first, 0);

                Grid.SetColumn(splitter, vertical ? 0 : 1);
                Grid.SetRow(splitter, vertical ? 1 : 0);

                Grid.SetColumn(second, vertical ? 0 : 2);
                Grid.SetRow(second, vertical ? 2 : 0);

                splitter.ResizeBehavior = GridResizeBehavior.PreviousAndNext;
                splitter.ResizeDirection = vertical ? GridResizeDirection.Rows : GridResizeDirection.Columns;
            }
            else
            {
                Grid.SetRowSpan(first, 3);
                Grid.SetColumnSpan(first, 3);

                splitter.Visibility = Visibility.Hidden;
                second.Visibility = Visibility.Hidden;
            }
        }

        private void CollapseSecondaryView()
        {
            collapseSecondaryView = true;
            SetupSplitter();
        }
    }
}