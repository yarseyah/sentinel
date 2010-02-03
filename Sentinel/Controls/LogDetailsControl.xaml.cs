#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Sentinel.Filters;
using Sentinel.Highlighting;
using Sentinel.Logger;
using Sentinel.Preferences;
using Sentinel.Services;
using Sentinel.Support;
using Sentinel.ViewModels;

#endregion

namespace Sentinel.Controls
{

    #region Using directives

    #endregion

    /// <summary>
    /// Interaction logic for LogDetailsControl.xaml
    /// </summary>
    public partial class LogDetailsControl
    {
        private readonly ILogger log;

        private readonly ILogManager logManager = ServiceLocator.Instance.Get<ILogManager>();

        private readonly IUserPreferences preferences = ServiceLocator.Instance.Get<IUserPreferences>();

        private readonly IQuickHighlighter searchHighlighter;

        public LogDetailsControl(string logName)
        {
            InitializeComponent();

            log = logManager.Get(logName);

            Details = new DetailsViewModel(log);
            Activity = new ActivityMonitoringViewModel(log);

            Details.Entries.CollectionChanged += EntriesCollectionChanged;

            if (preferences is INotifyPropertyChanged)
            {
                (preferences as INotifyPropertyChanged).PropertyChanged += PreferencesChanged;
            }

            Clear = new DelegateCommand(
                e =>
                    {
                        Details.Clear();
                        Activity.Clear();
                    });
            ClearActivity = new DelegateCommand(e => Activity.Clear());

            Save = new DelegateCommand(a => Details.Save(), e => Details.CanSave);

            Highlight = ServiceLocator.Instance.Get<IHighlightingService>();
            Filters = ServiceLocator.Instance.Get<IFilteringService>();
            searchHighlighter = ServiceLocator.Instance.Get<IQuickHighlighter>();

            SetupSplitter();

            DataContext = this;
        }

        public ActivityMonitoringViewModel Activity { get; private set; }

        public ICommand Clear { get; private set; }

        public ICommand ClearActivity { get; private set; }

        public ICommand Save { get; private set; }

        public DetailsViewModel Details { get; private set; }

        public IFilteringService Filters { get; private set; }

        public IHighlightingService Highlight { get; private set; }

        public IUserPreferences Preferences
        {
            get
            {
                return preferences;
            }
        }

        public string Search
        {
            get
            {
                return searchHighlighter.Search;
            }

            set
            {
                searchHighlighter.Search = value;
            }
        }

        ~LogDetailsControl()
        {
            if (preferences is INotifyPropertyChanged)
            {
                (preferences as INotifyPropertyChanged).PropertyChanged -= PreferencesChanged;
            }
        }

        private void EntriesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Details.ScrollToNewest)
            {
                lock (log.Entries)
                {
                    ScrollingHelper.ScrollToEnd(Dispatcher, LogMessages.messages);
                }
            }
        }

        private void PreferencesChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedTypeOption")
            {
                // Get the first column in logDetails and check it is a fixed-width column.
                // logDetails.
                if (LogMessages.messages != null)
                {
                    GridView view = LogMessages.messages.View as GridView;
                    if (view != null && view.Columns[0] is FixedWidthColumn)
                    {
                        FixedWidthColumn fixedColumn = (FixedWidthColumn) view.Columns[0];
                        switch (preferences.SelectedTypeOption)
                        {
                            case 0:
                                fixedColumn.FixedWidth = 0;
                                break;
                            case 1:
                                fixedColumn.FixedWidth = 30;
                                break;
                            case 2:
                                fixedColumn.FixedWidth = 60;
                                break;
                            case 3:
                                fixedColumn.FixedWidth = 90;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            else if (e.PropertyName == "SelectedDateOption")
            {
                if (LogMessages.messages != null)
                {
                    GridView view = LogMessages.messages.View as GridView;
                    if (view != null)
                    {
                        GridViewColumn column = view.Columns[1];

                        string dateFormat = "r";
                        switch (preferences.SelectedDateOption)
                        {
                            case 0:
                                dateFormat = "r";
                                column.Width = 150;
                                break;
                            case 1:
                                dateFormat = "dd/MM/yyyy HH:mm:ss";
                                column.Width = 120;
                                break;
                            case 2:
                                dateFormat = "dddd, d MMM yyyy, HH:mm:ss";
                                column.Width = 160;
                                break;
                            case 3:
                                dateFormat = "HH:mm:ss";
                                column.Width = 60;
                                break;
                            case 4:
                                dateFormat = "HH:mm:ss,fff";
                                column.Width = 80;
                                break;
                            default:
                                break;
                        }

                        column.DisplayMemberBinding = new Binding("DateTime") {StringFormat = dateFormat};
                    }
                }
            }
            else if (e.PropertyName == "UseStackedLayout")
            {
                SetupSplitter();
            }
        }

        private void SetupSplitter()
        {
            bool vertical = preferences.UseStackedLayout;

            int rowSpan = vertical ? 1 : 3;
            int colSpan = vertical ? 3 : 1;

            Grid.SetRowSpan(first, rowSpan);
            Grid.SetRowSpan(splitter, rowSpan);
            Grid.SetRowSpan(second, rowSpan);

            Grid.SetColumnSpan(first, colSpan);
            Grid.SetColumnSpan(splitter, colSpan);
            Grid.SetColumnSpan(second, colSpan);

            splitter.Width = vertical ? Int32.MaxValue : 5;
            splitter.Height = vertical ? 5 : Int32.MaxValue;
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
    }
}