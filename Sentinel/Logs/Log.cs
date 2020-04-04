namespace Sentinel.Logs
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;

    using Sentinel.Classification.Interfaces;
    using Sentinel.Interfaces;
    using Sentinel.Services;

    using WpfExtras;

    public class Log : ViewModelBase, ILogger
    {
        private readonly IClassifyingService<IClassifier> classifier;

        private readonly IUserPreferences preferences;

        private readonly List<ILogEntry> entries = new List<ILogEntry>();

        private readonly List<ILogEntry> newEntries = new List<ILogEntry>();

        private bool enabled = true;

        private string name;

        public Log()
        {
            Entries = entries;
            NewEntries = newEntries;

            classifier = ServiceLocator.Instance.Get<IClassifyingService<IClassifier>>();
            preferences = ServiceLocator.Instance.Get<IUserPreferences>();

            // Observe the NewEntries to maintain a full history.
            PropertyChanged += OnPropertyChanged;
        }

        public IEnumerable<ILogEntry> Entries { get; private set; }

        public bool Enabled
        {
            get => enabled;

            set
            {
                if (enabled != value)
                {
                    enabled = value;
                    OnPropertyChanged(nameof(Enabled));
                }
            }
        }

        public string Name
        {
            get => name;

            set
            {
                if (value != name)
                {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public IEnumerable<ILogEntry> NewEntries { get; private set; }

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

            OnPropertyChanged(nameof(Entries));
            OnPropertyChanged(nameof(NewEntries));
            GC.Collect();
        }

        public void AddBatch(Queue<ILogEntry> incomingEntries)
        {
            if (!enabled || incomingEntries.Count <= 0)
            {
                return;
            }

            var processed = new Queue<ILogEntry>();
            while (incomingEntries.Count > 0)
            {
                if (classifier != null)
                {
                    var entry = classifier.Classify(incomingEntries.Dequeue());
                    processed.Enqueue(entry);
                }
            }

            lock (newEntries)
            {
                newEntries.Clear();
                newEntries.AddRange(processed);
            }

            OnPropertyChanged(nameof(NewEntries));
        }

        public void LimitMessageCount(int maximumMessages)
        {
            lock (entries)
            {
                var messages = entries.Count;
                var excessMessages = messages - maximumMessages;

                if (excessMessages > 0)
                {
                    entries.RemoveRange(0, excessMessages);
                }
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "NewEntries")
            {
                lock (newEntries)
                {
                    lock (entries)
                    {
                        var entriesToAppend = newEntries.ToList();

                        // Look for any special command
                        if (preferences != null && preferences.EnableClearCommand)
                        {
                            if (entriesToAppend.Any(entry => entry.Description == preferences.ClearCommandMatchText))
                            {
                                Trace.WriteLine("!!!!!!!!!!!! CLEAR COMMAND FOUND !!!!!!!!!!");
                            }

                            var indexOfClear = entriesToAppend.FindLastIndex(
                                entry => entry.Description == preferences.ClearCommandMatchText);
                            if (indexOfClear != -1)
                            {
                                Trace.WriteLine(
                                    $"Clear command found (message {indexOfClear} of {newEntries.Count} incoming messages)");
                                entriesToAppend = newEntries.Skip(indexOfClear + 1).ToList();
                                Trace.WriteLine($"Message buffer of {entries.Count} messages being cleared");

                                entries.Clear();
                                OnPropertyChanged(nameof(Entries));

                                Debug.Assert(entries.Count == 0, "should have cleared entries");

                                newEntries.Clear();
                                newEntries.AddRange(entriesToAppend);
                                OnPropertyChanged(nameof(NewEntries));

                                return;
                            }
                        }

                        entries.AddRange(entriesToAppend);
                    }

                    OnPropertyChanged(nameof(Entries));
                }
            }
        }
    }
}