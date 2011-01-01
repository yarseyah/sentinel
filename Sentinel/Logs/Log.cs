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
using System.ComponentModel;
using Sentinel.Classification.Interfaces;
using Sentinel.Interfaces;
using Sentinel.Logs.Interfaces;
using Sentinel.Services;
using Sentinel.Support.Mvvm;

#endregion

namespace Sentinel.Logs
{
    public class Log : ViewModelBase, ILogger
    {
        private readonly IClassifierService classifier;

        private readonly List<LogEntry> entries = new List<LogEntry>();

        private readonly List<LogEntry> newEntries = new List<LogEntry>();

        private string name;

        public Log()
        {
            Entries = entries;
            NewEntries = newEntries;

            classifier = ServiceLocator.Instance.Get<IClassifierService>();

            // Observe the NewEntries to maintain a full history.
            PropertyChanged += OnPropertyChanged;
        }

        #region ILogger Members

        public IEnumerable<LogEntry> Entries { get; private set; }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if (value != name)
                {
                    name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public IEnumerable<LogEntry> NewEntries { get; private set; }

        public void Clear()
        {
            lock (entries)
            {
                entries.Clear();
            }

            lock (newEntries)
            {
                newEntries.Clear();
            }

            OnPropertyChanged("Entries");
            OnPropertyChanged("NewEntries");
            GC.Collect();
        }

        public void AddBatch(Queue<LogEntry> entries)
        {
            if (entries.Count <= 0) return;

            var processed = new Queue<LogEntry>();
            while (entries.Count > 0)
            {
                if (classifier != null)
                {
                    LogEntry entry = classifier.Classify(entries.Dequeue());
                    processed.Enqueue(entry);
                }
            }

            lock (newEntries)
            {
                newEntries.Clear();
                newEntries.AddRange(processed);
            }

            OnPropertyChanged("NewEntries");
        }

        #endregion

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "NewEntries")
            {
                lock (newEntries)
                {
                    lock (entries)
                    {
                        entries.AddRange(newEntries);
                    }

                    OnPropertyChanged("Entries");
                }
            }
        }
    }
}