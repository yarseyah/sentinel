namespace Sentinel.Views.Gui
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows.Controls;
    using System.Windows.Threading;
    using Sentinel.Extractors.Interfaces;
    using Sentinel.Filters.Interfaces;
    using Sentinel.Interfaces;
    using Sentinel.Services;
    using Sentinel.Views.Interfaces;
    using WpfExtras;

    public class LogMessages : ViewModelBase, ILogViewer
    {
        public static readonly IViewInformation Info = new ViewInformation(ID, NAME);

        private const string ID = "f4d8c068-bf72-4b83-9d4a-1cd8a89fea11";

        private const string NAME = "Log viewer";

        private const string DESCRIPTION = "Traditional row based log view with highlighting and incremental search.";

        private readonly IFilteringService<IFilter> filteringService;

        private readonly IExtractingService<IExtractor> extractingService;

        private readonly Queue<ILogEntry> pendingAdditions = new Queue<ILogEntry>();

        private readonly LogMessagesControl presenter;

        private bool clearPending;

        private int filteredCount;

        private ILogger logger;

        private bool rebuildList;

        private string status;

        private int unfilteredCount;

        private bool autoScroll = true;

        public LogMessages()
        {
            ((ViewInformation)Info).Description = DESCRIPTION;
            presenter = new LogMessagesControl
            {
                DataContext = this,
            };

            Messages = new ObservableCollection<ILogEntry>();

            PropertyChanged += PropertyChangedHandler;

            var dt = new DispatcherTimer(DispatcherPriority.Normal)
            {
                Interval = TimeSpan.FromMilliseconds(200),
            };
            dt.Tick += UpdateTick;
            dt.Start();

            filteringService = ServiceLocator.Instance.Get<IFilteringService<IFilter>>();
            if (filteringService != null)
            {
                if (filteringService is INotifyPropertyChanged notify)
                {
                    notify.PropertyChanged += (sender, e) => ApplyFiltering();
                }
            }

            extractingService = ServiceLocator.Instance.Get<IExtractingService<IExtractor>>();
            if (extractingService != null)
            {
                if (extractingService is INotifyPropertyChanged notify)
                {
                    notify.PropertyChanged += (sender, e) => ApplyExtracting();
                }
            }

            Preferences = ServiceLocator.Instance.Get<IUserPreferences>();
            if (Preferences != null)
            {
                if (Preferences is INotifyPropertyChanged notify)
                {
                    notify.PropertyChanged += (sender, args) =>
                    {
                        var prop = args.PropertyName;
                        switch (prop)
                        {
                            case "SelectedTimeFormatOption":
                            case "ConvertUtcTimesToLocalTimeZone":
                            case "SelectedDateOption":
                                rebuildList = true;
                                break;
                        }
                    };
                }
            }

            InitialiseToolbar();
        }

        public ObservableCollection<ILogEntry> Messages { get; private set; }

        /// <summary>
        /// Gets or sets the count of filtered entries.
        /// </summary>
        public int FilteredCount
        {
            get => filteredCount;

            set
            {
                if (filteredCount != value)
                {
                    filteredCount = value;
                    OnPropertyChanged(nameof(FilteredCount));
                }
            }
        }

        /// <summary>
        /// Gets or sets the count of unfiltered entries.
        /// </summary>
        public int UnfilteredCount
        {
            get => unfilteredCount;

            set
            {
                if (unfilteredCount != value)
                {
                    unfilteredCount = value;
                    OnPropertyChanged(nameof(UnfilteredCount));
                }
            }
        }

        public string Status
        {
            get => status;

            private set
            {
                if (status == value)
                {
                    return;
                }

                status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        /// <summary>
        /// Gets the name of a LogViewer.
        /// </summary>
        public string Name => Info.Name;

        public IEnumerable<ILogViewerToolbarButton> ToolbarItems { get; private set; }

        public ILogger Logger
        {
            get => logger;

            private set
            {
                if (logger != value)
                {
                    logger = value;
                    OnPropertyChanged(nameof(Logger));
                }
            }
        }

        /// <summary>
        /// Gets the Presenter control for a log viewer.
        /// </summary>
        public Control Presenter => presenter;

        private IUserPreferences Preferences { get; }

        public void SetLogger(ILogger newLogger)
        {
            Logger = newLogger;
        }

        private void PauseMessagesHandler(object obj)
        {
            Debug.Assert(logger != null, "Logger has not been instantiated");
            logger.Enabled = !logger.Enabled;
        }

        private void InitialiseToolbar()
        {
            var autoScrollButton = new LogViewerToolbarButton(
                "Auto-Scroll",
                "Automatically scroll to show the newest entry",
                true,
                new DelegateCommand(e => autoScroll = !autoScroll))
            {
                IsChecked = autoScroll,
                ImageIdentifier = "ScrollDown",
            };

            var clearButton = new LogViewerToolbarButton(
                "Clear",
                "Clear the log messages from the display",
                false,
                new DelegateCommand(e => clearPending = true))
            {
                ImageIdentifier = "Clear",
            };

            var pauseButton = new LogViewerToolbarButton(
                "Pause",
                "Pause the addition of messages to the display",
                true,
                new DelegateCommand(PauseMessagesHandler))
            {
                IsChecked = false,
                ImageIdentifier = "Pause",
            };

            var toolbar = new ObservableCollection<ILogViewerToolbarButton>
            {
                autoScrollButton,
                clearButton,
                pauseButton,
            };

            ToolbarItems = toolbar;
        }

        /// <summary>
        /// Apply filtering to the the collection of log entries.
        /// </summary>
        private void ApplyFiltering()
        {
            lock (Messages)
            {
                Trace.WriteLine("Applying filters...");

                // About to get the full data set from the LogEntriesManager,
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

        /// <summary>
        /// Append new log entry, as long as it wouldn't normally have been filtered.
        /// </summary>
        /// <param name="entry">Entry to add.</param>
        private void AddIfPassesFilters(ILogEntry entry)
        {
            lock (Messages)
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
        }

        private void ApplyExtracting()
        {
            lock (Messages)
            {
                Trace.WriteLine("Applying extractors...");

                // About to get the full data set from the LogEntriesManager,
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
            if (Logger == null)
            {
                return;
            }

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

                        if (Preferences?.LimitMessages ?? false)
                        {
                            var limitAsString = Preferences?.MaximumMessageCount;
                            if (int.TryParse(limitAsString, out var limit))
                            {
                                Logger.LimitMessageCount(limit);

                                // Ensure filtered view is also limited.
                                var messages = Messages.Count;
                                var excessMessages = messages - limit;

                                if (excessMessages > 0)
                                {
                                    for (; excessMessages > 0; excessMessages--)
                                    {
                                        Messages.RemoveAt(0);
                                    }
                                }
                            }
                        }
                    }
                }

                if (autoScroll)
                {
                    presenter.ScrollToEnd();
                }
            }

            FilteredCount = Messages.Count;
            UnfilteredCount = Logger.Entries.Count();
        }

        private void LoggerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "NewEntries")
            {
                lock (Logger.NewEntries)
                {
                    lock (pendingAdditions)
                    {
                        foreach (var entry in Logger.NewEntries)
                        {
                            pendingAdditions.Enqueue(entry);
                        }
                    }
                }
            }
            else if (e.PropertyName == "Entries")
            {
                lock (Messages)
                {
                    // Determine whether a clear has been instigated
                    if (Messages.Count > Logger.Entries.Count())
                    {
                        Messages.Clear();
                    }
                }
            }
            else if (e.PropertyName == "Enabled")
            {
                var pauseButton = ToolbarItems.FirstOrDefault(c => c.Label == "Pause");
                if (pauseButton != null)
                {
                    pauseButton.IsChecked = !logger.Enabled;
                }
            }
        }

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Logger")
            {
                lock (Messages)
                {
                    // Get rid of any existing messages and populate with messages
                    // from newly bound structure (if any).
                    Messages.Clear();
                }

                // Register to the logger.
                logger.PropertyChanged += LoggerPropertyChanged;
            }
            else if (e.PropertyName == "FilteredCount" || e.PropertyName == "UnfilteredCount")
            {
                lock (Messages)
                {
                    if (!Logger.Entries.Any())
                    {
                        Messages.Clear();
                    }

                    var filtered = FilteredCount < UnfilteredCount;
                    Status = filtered
                        ? $"{FilteredCount} of {UnfilteredCount} Messages [Filters Applied]"
                        : $"{UnfilteredCount} Messages";
                }
            }
        }
    }
}