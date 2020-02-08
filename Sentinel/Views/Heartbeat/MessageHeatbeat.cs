namespace Sentinel.Views.Heartbeat
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Controls;
    using System.Windows.Threading;

    using Sentinel.Interfaces;
    using Sentinel.Support.Wpf;
    using Sentinel.Views.Interfaces;

    using WpfExtras;

    public class MessageHeatBeat : ViewModelBase, ILogViewer
    {
        public static readonly string Id = "f1da010a-bd8f-4957-a16d-2f3ada1e40f6";

        public static readonly IViewInformation Info = new ViewInformation(Id, "Message Heartbeat");

        private const int MaxiumHistory = 200;

        private const int SamplePeriod = 1000;

        private readonly ObservableDictionary<string, ObservableCollection<int>> historicalData =
            new ObservableDictionary<string, ObservableCollection<int>>();

        private readonly Dictionary<string, int> liveData = new Dictionary<string, int>();

        private HeartbeatControl presenter;

        private ILogger logger;

        public MessageHeatBeat()
        {
            ((ViewInformation)Info).Description = "Displays a heartbeat graph based upon the incoming message type.";

            presenter = new HeartbeatControl { Data = historicalData };

            // Register an interest in changes to self, so that when the caller changes
            // properties on the view model, appropriate reactions can be preformed.
            PropertyChanged += PropertyChangedHandler;

            var samplePeriodTimer = new DispatcherTimer(DispatcherPriority.Normal)
                                        {
                                            Interval = TimeSpan.FromMilliseconds(SamplePeriod),
                                        };
            samplePeriodTimer.Tick += SampleTick;
            samplePeriodTimer.Start();
        }

        public ObservableCollection<ILogEntry> Messages { get; private set; }

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
                if (logger != value)
                {
                    if (logger != null)
                    {
                        logger.PropertyChanged -= LoggerPropertyChanged;
                    }

                    // If new logger isn't null, register to it.
                    if (value != null)
                    {
                        value.PropertyChanged += LoggerPropertyChanged;
                    }

                    logger = value;
                    OnPropertyChanged(nameof(Logger));
                }

                // TODO: Unregister from existing logger (if not null)
            }
        }

        /// <summary>
        /// Gets the Presenter control for a log viewer.
        /// </summary>
        public Control Presenter => presenter;

        public string Status => string.Empty;

        public IEnumerable<ILogViewerToolbarButton> ToolbarItems => null;

        public ObservableDictionary<string, ObservableCollection<int>> Data => historicalData;

        public void SetLogger(ILogger newLogger)
        {
            Logger = newLogger;
        }

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Logger")
            {
                // Need to purge all historical/live data as it doesn't match the associated logger.
                PurgeData();
            }
        }

        private void LoggerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "NewEntries")
            {
                lock (Logger.NewEntries)
                {
                    lock (liveData)
                    {
                        foreach (ILogEntry entry in Logger.NewEntries)
                        {
                            if (liveData.ContainsKey(entry.Type))
                            {
                                liveData[entry.Type]++;
                            }
                            else
                            {
                                liveData[entry.Type] = 1;
                            }
                        }
                    }
                }
            }
        }

        private void SampleTick(object sender, EventArgs e)
        {
            lock (liveData)
            {
                lock (Data)
                {
                    // Push out old data.
                    foreach (KeyValuePair<string, ObservableCollection<int>> pair in Data)
                    {
                        if (pair.Value.Count() >= MaxiumHistory)
                        {
                            pair.Value.RemoveAt(0);
                        }

                        // In cases where there is no liveData, need to set new value to zero
                        if (!liveData.ContainsKey(pair.Key))
                        {
                            pair.Value.Add(0);
                        }
                    }

                    // Push in new data
                    foreach (var dataPoint in liveData)
                    {
                        if (!Data.ContainsKey(dataPoint.Key))
                        {
                            Data.Add(dataPoint.Key, new ObservableCollection<int>());
                            for (int i = 0; i < MaxiumHistory - 1; i++)
                            {
                                Data[dataPoint.Key].Add(0);
                            }
                        }

                        Data[dataPoint.Key].Add(dataPoint.Value);
                    }
                }

                // Empty the collection
                liveData.Clear();

                OnPropertyChanged(nameof(Data));
            }
        }

        private void PurgeData()
        {
            lock (Data)
            {
                Data.Clear();
            }

            lock (liveData)
            {
                liveData.Clear();
            }
        }
    }
}