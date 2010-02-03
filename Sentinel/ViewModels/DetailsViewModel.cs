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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;
using Sentinel.Filters;
using Sentinel.Logger;
using Sentinel.Services;
using Sentinel.Support;

#endregion

namespace Sentinel.ViewModels
{

    #region Using directives

    #endregion

    /// <summary>
    /// View-model representing the detailed log-entries for a logger.
    /// </summary>
    public class DetailsViewModel : ViewModelBase
    {
        private readonly ILogger logEntriesManager;

        private readonly Queue<LogEntry> pendingAdditions = new Queue<LogEntry>();

        private bool clearPending;

        private int filteredCount;

        private ObservableCollection<LogEntry> filteredDetails = new ObservableCollection<LogEntry>();

        private bool rebuildList;

        private bool scrollToNewest = true;

        private int unfilteredCount;

        /// <summary>
        /// Initializes a new instance of the DetailsViewModel class.
        /// </summary>
        /// <param name="logEnties">Logger to associate this view model with.</param>
        public DetailsViewModel(ILogger logEnties)
        {
            logEntriesManager = logEnties;

            // Register self as observer to the logEntriesManager's OnPropertyChangeEvent.
            if (logEnties is INotifyPropertyChanged)
            {
                (logEnties as INotifyPropertyChanged).PropertyChanged += LogEntriesManagerPropertyChanged;
            }

            DispatcherTimer dt = new DispatcherTimer(DispatcherPriority.Normal)
                                     {
                                         Interval = TimeSpan.FromMilliseconds(200)
                                     };
            dt.Tick += UpdateTick;
            dt.Start();

            FilteringService = ServiceLocator.Instance.Get<IFilteringService>();
            if (FilteringService != null && FilteringService is INotifyPropertyChanged)
            {
                INotifyPropertyChanged notify = FilteringService as INotifyPropertyChanged;
                notify.PropertyChanged += (sender, e) => ApplyFiltering();
            }

            ApplyFiltering();
        }

        /// <summary>
        /// Gets the collection of log entries.
        /// </summary>
        public ObservableCollection<LogEntry> Entries
        {
            get
            {
                return filteredDetails;
            }

            private set
            {
                if (filteredDetails != value)
                {
                    filteredDetails = value;
                    OnPropertyChanged("Entries");
                }
            }
        }

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
        /// Gets the instance reference to the current filtering service.
        /// </summary>
        public IFilteringService FilteringService { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the listview should be scrolled to show 
        /// newly added entries.
        /// </summary>
        public bool ScrollToNewest
        {
            get
            {
                return scrollToNewest;
            }

            set
            {
                if (scrollToNewest != value)
                {
                    scrollToNewest = value;
                    OnPropertyChanged("ScrollToNewest");
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

        /// <summary>
        /// Gets a value indicating whether the current details view be saved.
        /// </summary>
        public bool CanSave
        {
            get
            {
                return Entries.Count > 0;
            }
        }

        /// <summary>
        /// Empty the collections of log entries.
        /// </summary>
        public void EmptyCollection()
        {
            logEntriesManager.Clear();
            lock (filteredDetails)
            {
                filteredDetails.Clear();
            }
        }

        /// <summary>
        /// Saves the current view.
        /// </summary>
        public void Save()
        {
            LogWriter writer = ServiceLocator.Instance.Get<LogWriter>();
            writer.Write(Entries);
        }

        /// <summary>
        /// Clear the log entries.
        /// </summary>
        internal void Clear()
        {
            // Tell logEntriesManager to clear its buffers.
            logEntriesManager.Clear();

            // Inform timer tick routine that a clear is pending.
            clearPending = true;
        }

        /// <summary>
        /// Append new log entry, as long as it wouldn't normally have been filtered.
        /// </summary>
        /// <param name="entry">Entry to add.</param>
        private void AddIfPassesFilters(LogEntry entry)
        {
            // If no filtering service, then assume it passes.
            if (FilteringService == null)
            {
                filteredDetails.Add(entry);
            }
            else
            {
                if (FilteringService.IsFiltered(entry))
                {
                    filteredDetails.Add(entry);
                }
            }
        }

        /// <summary>
        /// Apply filtering to the the collection of log entries.
        /// </summary>
        private void ApplyFiltering()
        {
            lock (logEntriesManager.Entries)
            {
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

        /// <summary>
        /// Callback method which is called when the properties of the view-model
        /// <c>LogEntriesManager</c> have changed.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void LogEntriesManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "NewEntries")
            {
                lock (logEntriesManager.NewEntries)
                {
                    lock (pendingAdditions)
                    {
                        foreach (LogEntry entry in logEntriesManager.NewEntries)
                        {
                            pendingAdditions.Enqueue(entry);
                        }
                    }
                }
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
            if (clearPending || rebuildList)
            {
                rebuildList = clearPending = false;

                // If rebuilding the list, any additions to the pendingAdditions
                // made since the "rebuildList" variable was set to true will
                // not be needed, throw them away to avoid duplication.
                lock (pendingAdditions)
                {
                    pendingAdditions.Clear();
                }

                lock (filteredDetails)
                {
                    filteredDetails.Clear();

                    lock (logEntriesManager.Entries)
                    {
                        foreach (LogEntry entry in logEntriesManager.Entries)
                        {
                            AddIfPassesFilters(entry);
                        }
                    }
                }
            }
            else if (pendingAdditions.Count > 0)
            {
                lock (pendingAdditions)
                {
                    lock (filteredDetails)
                    {
                        while (pendingAdditions.Count > 0)
                        {
                            LogEntry entry = pendingAdditions.Dequeue();
                            AddIfPassesFilters(entry);
                        }
                    }
                }
            }

            FilteredCount = filteredDetails.Count();
            UnfilteredCount = logEntriesManager.Entries.Count();
        }
    }
}