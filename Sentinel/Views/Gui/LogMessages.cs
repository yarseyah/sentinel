#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Threading;
using Sentinel.Filters.Interfaces;
using Sentinel.Interfaces;
using Sentinel.Logs.Interfaces;
using Sentinel.Services;
using Sentinel.Support.Mvvm;
using Sentinel.Views.Interfaces;

#endregion

namespace Sentinel.Views.Gui
{
    using Sentinel.Filters;
    using Sentinel.Extractors.Interfaces;

    public class LogMessages
        : ViewModelBase
          , ILogViewer
    {
        private const string ID = "f4d8c068-bf72-4b83-9d4a-1cd8a89fea11";
        
        private const string NAME = "Log viewer";
        
        private const string DESCRIPTION = "Traditional row based log view with highlighting and incremental search.";

        public static readonly IViewInformation Info = new ViewInformation(ID, NAME);

        private readonly IFilteringService<IFilter> filteringService;
        private readonly IExtractingService<IExtractor> extractingService;
        private readonly Queue<ILogEntry> pendingAdditions = new Queue<ILogEntry>();
        private readonly LogMessagesControl presenter;

        private bool clearPending;        
        private int filteredCount;
        private int extractedCount;
        private ILogger logger;
        private bool rebuildList;
        private string status;
        private int unfilteredCount;
        private bool autoscroll = true;

        public LogMessages()
        {
            ((ViewInformation) Info).Description = DESCRIPTION;
            presenter = new LogMessagesControl { DataContext = this };

            Messages = new ObservableCollection<ILogEntry>();
            PropertyChanged += PropertyChangedHandler;

            DispatcherTimer dt = new DispatcherTimer(DispatcherPriority.Normal)
                                     {
                                         Interval = TimeSpan.FromMilliseconds(200)
                                     };
            dt.Tick += UpdateTick;
            dt.Start();

            filteringService = ServiceLocator.Instance.Get<IFilteringService<IFilter>>();
            if (filteringService != null)
            {
                var notify = filteringService as INotifyPropertyChanged;
                if (notify != null)
                {
                    notify.PropertyChanged += (sender, e) => ApplyFiltering();
                }
            }

            extractingService = ServiceLocator.Instance.Get<IExtractingService<IExtractor>>();
            if (extractingService != null)
            {
                var notify = extractingService as INotifyPropertyChanged;
                if (notify != null)
                {
                    notify.PropertyChanged += (sender, e) => ApplyExtracting();
                }
            }

            InitialiseToolbar();
        }

        private void InitialiseToolbar()
        {
            var autoscrollButton = new LogViewerToolbarButton(
                "Auto-Scroll",
                "Automatically scroll to show the newest entry",
                true,
                new DelegateCommand(e => autoscroll = !autoscroll));
            autoscrollButton.IsChecked = autoscroll;
            autoscrollButton.ImageIdentifier = "ScrollDown";

            var clearButton = new LogViewerToolbarButton(
                "Clear",
                "Clear the log messages from the display",
                false,
                new DelegateCommand(e => clearPending = true));
            clearButton.ImageIdentifier = "Clear";

            var pauseButton = new LogViewerToolbarButton(
                "Pause",
                "Pause the addition of messages to the display",
                true,
                new DelegateCommand(PauseMessagesHandler));
            pauseButton.IsChecked = false;
            pauseButton.ImageIdentifier = "Pause";

            var toolbar = new ObservableCollection<ILogViewerToolbarButton>
                              {
                                  autoscrollButton,
                                  clearButton,
                                  pauseButton
                              };

            ToolbarItems = toolbar;
        }

        private void PauseMessagesHandler(object obj)
        {
            Debug.Assert(logger != null, "Logger has not been instantiated");
            logger.Enabled = !logger.Enabled;
        }

        public ObservableCollection<ILogEntry> Messages { get; private set; }

        /// <summary>
        /// Gets the count of filtered entries.
        /// </summary>
        public int FilteredCount
        {
            get
            {
                return filteredCount;
            }

            private set
            {
                if (filteredCount != value)
                {
                    filteredCount = value;
                    OnPropertyChanged("FilteredCount");
                }
            }
        }

        /// <summary>
        /// Gets the count of extracted entries.
        /// </summary>
        public int ExtractedCount
        {
            get
            {
                return extractedCount;
            }
            private set
            {
                if (extractedCount != value)
                {
                    extractedCount = value;
                    OnPropertyChanged("ExtractedCount");
                }
            }
        }

        /// <summary>
        /// Gets the count of unfiltered entries.
        /// </summary>
        public int UnfilteredCount
        {
            get
            {
                return unfilteredCount;
            }

            private set
            {
                if (unfilteredCount != value)
                {
                    unfilteredCount = value;
                    OnPropertyChanged("UnfilteredCount");
                }
            }
        }

        #region ILogViewer Members

        public string Status
        {
            get
            {
                return status;
            }
            private set
            {
                if (status == value) return;
                status = value;
                OnPropertyChanged("Status");
            }
        }

        #endregion

        
        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Logger")
            {
                // Get rid of any existing messages and populate with messages
                // from newly bound structure (if any).
                Messages.Clear();

                // Register to the logger.
                logger.PropertyChanged += LoggerPropertyChanged;
            }
            else if (e.PropertyName == "FilteredCount" || e.PropertyName == "UnfilteredCount")
            {
                bool filtered = FilteredCount < UnfilteredCount;
                Status = filtered
                             ? string.Format("{0} of {1} Messages [Filters Applied]", FilteredCount, UnfilteredCount)
                             : string.Format("{0} Messages", UnfilteredCount);
            }
        }

        private void LoggerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "NewEntries")
            {
                lock (Logger.NewEntries)
                {
                    lock (pendingAdditions)
                    {
                        foreach (ILogEntry entry in Logger.NewEntries)
                        {
                            pendingAdditions.Enqueue(entry);
                        }
                    }
                }
            }
            else if (e.PropertyName =="Enabled")
            {
                var pauseButton = ToolbarItems.FirstOrDefault(c => c.Label == "Pause");
                pauseButton.IsChecked = !logger.Enabled;
            }
        }

        /// <summary>
        /// Callback method called upon the delegate timer.  Rebuilds the list
        /// or appends new entries based upon the state of <c>clearPending</c>,
        /// <c>rebuildList</c> or the <c>pendingActions</c> collection containing
        /// entries.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">Argument for the timer callback.</param>
        private void UpdateTick(object sender, EventArgs e)
        {
            if (Logger == null) return;

            if (clearPending || rebuildList)
            {
                // If rebuilding the list, any additions to the pendingAdditions
                // made since the "rebuildList" variable was set to true will
                // not be needed, throw them away to avoid duplication.
                lock (pendingAdditions)
                {
                    pendingAdditions.Clear();
                }

                lock (Messages)
                {
                    Messages.Clear();

                    lock (Logger.Entries)
                    {
                        if (clearPending)
                        {
                            Logger.Clear();
                        }

                        foreach (var entry in Logger.Entries)
                        {
                            AddIfPassesFilters(entry);
                        }
                    }
                }

                // Reset the counters.
                FilteredCount = 0;
                UnfilteredCount = 0;

                rebuildList = clearPending = false;
            }
            else if (pendingAdditions.Count > 0)
            {
                lock (pendingAdditions)
                {
                    lock (Messages)
                    {
                        while (pendingAdditions.Count > 0)
                        {
                            var entry = pendingAdditions.Dequeue();
                            AddIfPassesFilters(entry);
                        }
                    }
                }

                if (autoscroll)
                {
                    presenter.ScrollToEnd();
                }
            }

            FilteredCount = Messages.Count();
            UnfilteredCount = Logger.Entries.Count();
        }

        /// <summary>
        /// Append new log entry, as long as it wouldn't normally have been filtered.
        /// </summary>
        /// <param name="entry">Entry to add.</param>
        private void AddIfPassesFilters(ILogEntry entry)
        {
            // If no filtering service or no extracting service, then assume it passes.
            if (filteringService == null || extractingService == null)
            {
                Messages.Add(entry);
            }
            else
            {
                if (!filteringService.IsFiltered(entry) && !extractingService.IsFiltered(entry))
                {                    
                    Messages.Add(entry);
                }
            }
        }

        /// <summary>
        /// Apply filtering to the the collection of log entries.
        /// </summary>
        private void ApplyFiltering()
        {
            lock (Messages)
            {
                Trace.WriteLine("Applying filters...");

                // About to get the full dataset from the LogEntriesManager,
                // therefore anything in the pendingQueue is unneeded as it
                // will already be in the complete collection and the incomplete
                // filtered copy of that list is going to be disposed.
                lock (pendingAdditions)
                {
                    pendingAdditions.Clear();
                }

                rebuildList = true;
            }
        }

        private void ApplyExtracting()
        {
            lock (Messages)
            {
                Trace.WriteLine("Applying extractors...");

                // About to get the full dataset from the LogEntriesManager,
                // therefore anything in the pendingQueue is unneeded as it
                // will already be in the complete collection and the incomplete
                // filtered copy of that list is going to be disposed.
                lock (pendingAdditions)
                {
                    pendingAdditions.Clear();
                }

                rebuildList = true;
            }
        }

        #region Implementation of ILogViewer

        /// <summary>
        /// Gets or sets the name of a LogViewer.
        /// </summary>
        public string Name
        {
            get
            {
                return Info.Name;
            }
        }

        public ILogger Logger
        {
            get
            {
                return logger;
            }
            private set
            {
                if (logger == value) return;
                logger = value;
                OnPropertyChanged("Logger");
            }
        }

        /// <summary>
        /// Gets or sets the Presenter control for a log viewer.
        /// </summary>
        public Control Presenter
        {
            get
            {
                return presenter;
            }
        }

        public void SetLogger(ILogger newLogger)
        {
            Logger = newLogger;
        }

        public IEnumerable<ILogViewerToolbarButton> ToolbarItems { get; private set; }

        #endregion
    }
}