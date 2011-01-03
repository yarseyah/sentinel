#region Using directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Sentinel.Interfaces;
using Sentinel.Logs.Interfaces;
using Sentinel.Providers.Interfaces;

#endregion

namespace Sentinel.Providers
{
    public class FileMonitoringProvider : ILogProvider
    {
        public const string ID = "1a2f8249-b390-4baa-ba5e-3d67804ba1ed";
        public const string NAME = "File Monitoring Provider";
        public const string DESCRIPTION = "Monitor a text file for new log entries.";

        public static readonly Guid Id = new Guid(ID);

        public static readonly ProviderInfo Info = new ProviderInfo(Id, NAME, DESCRIPTION);
        private readonly bool loadExistingContent;
        private readonly Regex patternMatching;

        private readonly Queue<LogEntry> pendingQueue = new Queue<LogEntry>();
        private readonly int refreshInterval = 250;
        private readonly List<string> usedGroupNames = new List<string>();
        private long bytesRead;

        protected BackgroundWorker purgeWorker;
        private BackgroundWorker worker;

        public FileMonitoringProvider(IProviderSettings settings)
        {
            Debug.Assert(settings is IFileMonitoringProviderSettings,
                         "The FileMonitoringProvider class expects configuration information "
                         + "to be of IFileMonitoringProviderSettings type");

            IFileMonitoringProviderSettings fileSettings = (IFileMonitoringProviderSettings) settings;
            FileName = fileSettings.FileName;
            Information = settings.Info;
            refreshInterval = fileSettings.RefreshPeriod;
            loadExistingContent = fileSettings.LoadExistingContent;
            patternMatching = new Regex(fileSettings.MessageDecoder, RegexOptions.Singleline | RegexOptions.Compiled);

            PredetermineGroupNames(fileSettings.MessageDecoder);

            worker = new BackgroundWorker();
            worker.DoWork += DoWork;
            worker.RunWorkerCompleted += DoWorkComplete;

            purgeWorker = new BackgroundWorker
                              {
                                  WorkerReportsProgress = true
                              };
            purgeWorker.DoWork += PurgeWorkerDoWork;
        }

        public string FileName
        {
            get;
            private set;
        }

        #region Implementation of ILogProvider

        public ProviderInfo Information
        {
            get;
            private set;
        }

        public ILogger Logger
        {
            get;
            set;
        }

        public void Start()
        {
            Debug.Assert(!string.IsNullOrEmpty(FileName), "Filename not specified");
            Debug.Assert(Logger != null, "No logger has been registered, this is required before starting a provider");

            Trace.WriteLine(string.Format("Starting of file-monitor upon {0}", FileName));
            worker.RunWorkerAsync();
            purgeWorker.RunWorkerAsync();
        }

        public void Close()
        {
            worker.CancelAsync();
            purgeWorker.CancelAsync();
        }

        public void Pause()
        {
            if (worker != null)
            {
                // TODO: need a better pause mechanism...
                Close();
            }
        }

        public string Name
        {
            get;
            set;
        }

        public bool IsActive
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        private void PurgeWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            while (!e.Cancel)
            {
                // Go to sleep.
                Thread.Sleep(refreshInterval);
                lock (pendingQueue)
                {
                    if (pendingQueue.Any())
                    {
                        Trace.WriteLine(string.Format("Adding a batch of {0} entries to the logger",
                                                      pendingQueue.Count()));
                        Logger.AddBatch(pendingQueue);
                        Trace.WriteLine("Done adding the batch");
                    }
                }
            }
        }

        private void PredetermineGroupNames(string messageDecoder)
        {
            string decoder = messageDecoder.ToLower();
            if (decoder.Contains("(?<description>")) usedGroupNames.Add("Description");
            if (decoder.Contains("(?<datetime>")) usedGroupNames.Add("DateTime");
            if (decoder.Contains("(?<type>")) usedGroupNames.Add("Type");
            if (decoder.Contains("(?<logger>")) usedGroupNames.Add("Logger");
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            // Read existing content.
            FileInfo fi = new FileInfo(FileName);

            // Keep hold of incomplete lines, if any.
            string incomplete = string.Empty;

            StringBuilder sb = new StringBuilder();

            if (!loadExistingContent)
            {
                bytesRead = fi.Length;
            }

            while (!e.Cancel)
            {
                if (fi.Exists)
                {
                    fi.Refresh();

                    long length = fi.Length;
                    if (length > bytesRead)
                    {
                        using (FileStream fs = fi.Open(FileMode.Open, FileAccess.Read, FileShare.Write))
                        {
                            long position = fs.Seek(bytesRead, SeekOrigin.Begin);
                            Debug.Assert(position == bytesRead, "Seek did not go to where we asked.");

                            // Calculate length of file.
                            long bytesToRead = length - position;
                            Debug.Assert(bytesToRead < Int32.MaxValue, "Too much data to read using this method!");

                            byte[] buffer = new byte[bytesToRead];

                            int bytesSuccessfullyRead = fs.Read(buffer, 0, (int) bytesToRead);
                            Debug.Assert(bytesSuccessfullyRead == bytesToRead, "Did not get as much as expected!");

                            // Put results into a buffer (prepend any unprocessed data retained from last read).
                            sb.Length = 0;
                            sb.Append(incomplete);
                            sb.Append(Encoding.ASCII.GetString(buffer, 0, bytesSuccessfullyRead));

                            using (StringReader sr = new StringReader(sb.ToString()))
                            {
                                while (sr.Peek() != -1)
                                {
                                    string line = sr.ReadLine();
                                    // Trace.WriteLine("Read: " + line);

                                    DecodeAndQueueMessage(line);
                                }
                            }

                            // Can we determine whether any tailing data was unprocessed?
                            bytesRead = position + bytesSuccessfullyRead;
                        }
                    }
                }

                Thread.Sleep(refreshInterval);
            }
        }

        private void DecodeAndQueueMessage(string message)
        {
            Debug.Assert(patternMatching != null, "Regular expression has not be set");
            Match m = patternMatching.Match(message);

            if (!m.Success)
            {
                Trace.WriteLine("Message decoding did not work!");
                return;
            }

            lock (pendingQueue)
            {
                LogEntry entry = new LogEntry();

                if (usedGroupNames.Contains("Description"))
                {
                    entry.Description = m.Groups["Description"].Value;
                }

                if (usedGroupNames.Contains("DateTime"))
                {
                    DateTime dt;
                    if (!DateTime.TryParse(m.Groups["DateTime"].Value, out dt))
                    {
                        Trace.WriteLine("Failed to parse date " + m.Groups["DateTime"].Value);
                    }
                    entry.DateTime = dt;
                }

                if (usedGroupNames.Contains("Type"))
                {
                    entry.Type = m.Groups["Type"].Value;
                }

                if (usedGroupNames.Contains("Logger"))
                {
                    entry.Source = m.Groups["Logger"].Value;
                }

                pendingQueue.Enqueue(entry);
            }
        }

        private void DoWorkComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            // TODO: brutal...
            worker = null;
        }
    }
}