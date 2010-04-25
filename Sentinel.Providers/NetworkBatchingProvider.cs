#region Using directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Sentinel.Interfaces;
using Sentinel.Logs.Interfaces;
using Sentinel.Providers.Interfaces;

#endregion

namespace Sentinel.Providers
{
    public abstract class NetworkBatchingProvider : INetworkProvider
    {
        protected readonly int maximumAge = 5;

        protected readonly Queue<string> pendingQueue = new Queue<string>();

        protected BackgroundWorker backgroundWorker;

        protected bool isActive;

        protected BackgroundWorker purgeWorker;

        protected NetworkBatchingProvider(IProviderSettings settings)
        {
            Debug.Assert(settings is NetworkSettings, "Expecting the provider settings to include network information");
            Name = settings.Name;

            NetworkSettings networkSettings = ((NetworkSettings) settings);
            Port = networkSettings.Port;
            IsUdp = networkSettings.IsUdp;
        }

        public int Port { get; private set; }

        public bool IsUdp { get; private set; }

        #region ILogProvider Members

        public ILogger Logger { get; set; }

        public bool IsActive
        {
            get
            {
                return isActive;
            }
        }

        public string Name { get; set; }

        public void Close()
        {
            // TODO: brutal...
            backgroundWorker.CancelAsync();
            backgroundWorker = null;
        }

        public void Pause()
        {
            if (backgroundWorker != null)
            {
                // TODO: need a better pause mechanism...
                Close();
            }
        }

        public abstract IProviderInfo Information { get; }

        public void Start()
        {
            Debug.Assert(Logger != null, "Logger needs to be set correctly");

            if (backgroundWorker == null)
            {
                backgroundWorker = new BackgroundWorker();

                if (IsUdp)
                {
                    backgroundWorker.DoWork += UdpBackgroundWorkerDoWork;
                }
                else
                {
                    backgroundWorker.DoWork += TcpBackgroundWorkerDoWork;
                }

                backgroundWorker.RunWorkerCompleted += BackgroundWorkerRunWorkerCompleted;
                backgroundWorker.RunWorkerAsync();

                purgeWorker = new BackgroundWorker
                                  {
                                      WorkerReportsProgress = true
                                  };
                purgeWorker.DoWork += PurgeWorkerDoWork;
                purgeWorker.RunWorkerAsync();
            }
            else
            {
                // TODO: need to tell background worker how to pause.
            }
        }

        #endregion

        protected abstract LogEntry DecodeEntry(string logEntry);

        protected abstract bool IsValidEntry(string logEntry);

        private void UdpBackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
#if PERFORMANCE_TESTING
            Trace.WriteLine("Starting UDP listener");
            int count = 0;
            int totalMissed = 0;
            Regex validatingRegex = new Regex("<log4(j|net):message>(?<count>\\d+)</log4(j|net):message>", RegexOptions.Compiled);
#endif

            UdpClient listener = null;
            try
            {
                listener = new UdpClient(Port);
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, Port);

                while (!e.Cancel)
                {
                    byte[] bytes = listener.Receive(ref endPoint);
                    string message = Encoding.ASCII.GetString(bytes, 0, bytes.Length);

#if PERFORMANCE_TESTING
                    Match m = validatingRegex.Match(message);
                    if (m.Success)
                    {
                        int messageCount = Int32.Parse(m.Groups["count"].Value);

                        if (messageCount != count)
                        {
                            totalMissed += messageCount - count;
                            double percentage = (100.0 / messageCount) * totalMissed;
                            Trace.WriteLine(
                                String.Format(
                                    "Expecting {0} but got {1}... missed {2} - Miss Percentage = {3:0.00}% or {4}",
                                    count,
                                    messageCount,
                                    messageCount - count,
                                    percentage,
                                    totalMissed));
                        }

                        count = messageCount + 1;
                    }
#endif

                    lock (pendingQueue)
                    {
                        pendingQueue.Enqueue(message);
                    }
                }
            }
            catch (Exception exception)
            {
                e.Result = exception;
            }
            finally
            {
                if (listener != null)
                {
                    listener.Close();
                }
            }
        }

        private void TcpBackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            TcpListener listener = null;

#if PERFORMANCE_TESTING
            Trace.WriteLine("Starting TCP listener");
            int count = 0;
            int totalMissed = 0;

            Regex validatingRegex = new Regex("<log4j:message>(?<count>\\d+)</log4j:message>", RegexOptions.Compiled);
#endif

            byte[] buffer = new byte[1024];

            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, Port);
                listener = new TcpListener(endPoint);
                listener.Start(100);

                while (!e.Cancel)
                {
                    TcpClient client = listener.AcceptTcpClient();

                    NetworkStream stream = client.GetStream();
                    int i;

                    while ((i = stream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        string message = Encoding.ASCII.GetString(buffer, 0, i);

#if PERFORMANCE_TESTING
                        Match m = validatingRegex.Match(message);
                        if (m.Success)
                        {
                            int messageCount = Int32.Parse(m.Groups["count"].Value);

                            if (messageCount != count)
                            {
                                totalMissed += messageCount - count;
                                double percentage = (100.0 / messageCount) * totalMissed;
                                Trace.WriteLine(
                                    String.Format(
                                        "Expecting {0} but got {1}... missed {2} - Miss Percentage = {3:0.00}% or {4}",
                                        count,
                                        messageCount,
                                        messageCount - count,
                                        percentage,
                                        totalMissed));
                            }

                            count = messageCount + 1;
                        }
#endif

                        lock (pendingQueue)
                        {
                            pendingQueue.Enqueue(message);
                        }
                    }

                    client.Close();
                }
            }
            catch (Exception exception)
            {
                e.Result = exception;
            }
            finally
            {
                if (listener != null)
                {
                    listener.Stop();
                }
            }
        }

        private void BackgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Trace.WriteLine(string.Format("RunWorkerCompleted - UdpListener {0} - {1}", Name, Port));
            Trace.WriteLine(string.Format("   - Cancelled = {0} ", e.Cancelled));
            Trace.WriteLine(string.Format("   - Error     = {0} ", e.Error));

            if (e.Result is Exception)
            {
                string errorMessage = string.Format(
                    "There was a problem with the UDP listener which was "
                    + " operating on port '{0}'\r\n"
                    + "\r\nThe following error message was returned from the " +
                    "exception:\r\n\r\n     \"{1}\"",
                    Port,
                    (e.Result as Exception).Message);

                throw new ApplicationException(errorMessage, e.Result as Exception);

                // TODO: Restore error handling logic.
                /*
                ServiceLocator.Instance.Get<IViewManager>().SetState(Name, LogViewerState.Stopped);

                MessageBox.Show(
                    errorMessage,
                    "Exception Caught",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                 */
            }
        }

        private void PurgeWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            while (!e.Cancel)
            {
                // Go to sleep.
                Thread.Sleep(maximumAge);

                if (Logger != null)
                {
                    if (pendingQueue.Count > 0)
                    {
                        Queue<LogEntry> processedQueue = new Queue<LogEntry>();

                        lock (pendingQueue)
                        {
                            while (pendingQueue.Count > 0)
                            {
                                string queuedMessage = pendingQueue.Dequeue();

                                // Need to validate the entry is indeed a log4j message format.
                                if (IsValidEntry(queuedMessage))
                                {
                                    processedQueue.Enqueue(DecodeEntry(queuedMessage));
                                }
                                else
                                {
                                    Trace.WriteLine("Invalid message : " + queuedMessage);
                                }
                            }

                            pendingQueue.Clear();
                        }

                        Logger.AddBatch(processedQueue);
                    }
                }
            }
        }
    }
}