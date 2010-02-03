#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#define RESORTING

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
using System.Windows.Threading;
using Sentinel.Logger;
using Sentinel.Preferences;
using Sentinel.Services;
using Sentinel.Support;

#endregion

namespace Sentinel.ViewModels
{

    #region Using directives

    #endregion

    public class ActivityMonitoringViewModel : ViewModelBase
    {
        private readonly Dictionary<string, Activity> data = new Dictionary<string, Activity>();

        private readonly ILogger log;

        private readonly Queue<Activity> pendingAdditions = new Queue<Activity>();

        public ActivityMonitoringViewModel(ILogger log)
        {
            this.log = log;

            Details = new ObservableCollection<Activity>();

            DispatcherTimer dt = new DispatcherTimer(DispatcherPriority.Normal)
                                     {
                                         Interval = TimeSpan.FromSeconds(1)
                                     };
            dt.Tick += TimerTick;
            dt.Start();

            Preferences = ServiceLocator.Instance.Get<IUserPreferences>();
            if (Preferences is INotifyPropertyChanged)
            {
                (Preferences as INotifyPropertyChanged).PropertyChanged
                    += (sender, e) =>
                           {
                               if (e.PropertyName == "UseLazyRebuild")
                               {
                                   UseLazyRebuild = Preferences.UseLazyRebuild;
                               }
                           };
            }

            // Register as an observer for new events
            if (log is INotifyPropertyChanged)
            {
                (log as INotifyPropertyChanged).PropertyChanged += EntriesPropertyChanged;
            }
        }

        public ObservableCollection<Activity> Details { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the sorting mechanism should use 
        /// the lazy rebuild mode.  
        /// <para>
        /// By lazy, it removes all existing values in the Details collection 
        /// and replaces them with the ones in the data dictionary.  This is inelegent 
        /// as it causes too much work and loss of the currently selected item.
        /// However, it forces the sorting to work correctly, which doesn't 
        /// get recalled when each new item is added to the collection in 
        /// non-lazy mode.
        /// </para>
        /// </summary>
        public bool UseLazyRebuild { get; set; }

        private IUserPreferences Preferences { get; set; }

        public void Clear()
        {
            lock (data)
            {
                data.Clear();
            }

            lock (Details)
            {
                Details.Clear();
            }

            OnPropertyChanged("Details");
            GC.Collect();
        }

        public void EntriesPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Only interested in new entries.
            if (e.PropertyName == "NewEntries")
            {
                lock (log.NewEntries)
                {
                    lock (data)
                    {
                        foreach (LogEntry logEntry in log.NewEntries)
                        {
                            string identifier = logEntry.System;
                            if (data.ContainsKey(identifier))
                            {
                                data[identifier].Update(logEntry);
                            }
                            else
                            {
                                if (UseLazyRebuild)
                                {
                                    data[identifier] = new Activity(logEntry);
                                }
                                else
                                {
                                    lock (pendingAdditions)
                                    {
                                        pendingAdditions.Enqueue(data[identifier] = new Activity(logEntry));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void TimerTick(object sender, EventArgs e)
        {
            lock (Details)
            {
                if (UseLazyRebuild)
                {
                    Details.Clear();
                    foreach (Activity value in data.Values)
                    {
                        Details.Add(value);
                    }

                    OnPropertyChanged("Details");
                }
                else
                {
                    foreach (Activity activity in Details)
                    {
                        activity.Tick();
                    }

                    // Add anything pending
                    lock (pendingAdditions)
                    {
                        while (pendingAdditions.Count > 0)
                        {
                            Details.Add(pendingAdditions.Dequeue());
                        }
                    }
                }
            }
        }
    }
}