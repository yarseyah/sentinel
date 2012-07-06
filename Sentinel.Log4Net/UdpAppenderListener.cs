namespace Sentinel.Log4Net
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Serialization;

    using Common.Logging;

    using Sentinel.Interfaces;
    using Sentinel.Interfaces.Providers;

    public class UdpAppenderListener : INetworkProvider
    {
        protected readonly Queue<string> PendingQueue = new Queue<string>();

        private const int PumpFrequency = 100;

        private static readonly XmlNamespaceManager NamespaceManager = new XmlNamespaceManager(new NameTable());

        public static readonly IProviderRegistrationRecord ProviderRegistrationInformation =
            new ProviderRegistrationInformation(new Log4NetUdpListenerProvider());

        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private readonly IUdpAppenderListenerSettings udpSettings;

        private CancellationTokenSource cancellationTokenSource;

        private Task udpListenerTask;

        private Task messagePumpTask;

        static UdpAppenderListener()
        {
            NamespaceManager.AddNamespace("log4net", "http://logging.apache.org/log4net");
        }

        public UdpAppenderListener(IProviderSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            udpSettings = settings as IUdpAppenderListenerSettings;
            if (udpSettings == null)
            {
                throw new ArgumentException("settings should be assignable to IUdpAppenderListenerSettings", "settings");
            }

            Information = ProviderRegistrationInformation.Info;
        }

        public IProviderInfo Information { get; private set; }

        public ILogger Logger { get; set; }

        public string Name { get; set; }

        public bool IsActive
        {
            get
            {
                return udpListenerTask != null && udpListenerTask.Status == TaskStatus.Running;
            }
        }

        public int Port { get; private set; }

        public void Start()
        {
            Log.Debug("Start requested");

            if (udpListenerTask == null || udpListenerTask.IsCompleted)
            {
                cancellationTokenSource = new CancellationTokenSource();
                var token = cancellationTokenSource.Token;

                udpListenerTask = Task.Factory.StartNew(SocketListener, token);
                messagePumpTask = Task.Factory.StartNew(MessagePump, token);
            }
            else
            {
                Log.Warn("UDP listener task is already active and can not be started again.");
            }
        }

        public void Pause()
        {
            Log.Debug("Pause requested");
            if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
            {
                Log.Debug("Cancellation token triggered");
                cancellationTokenSource.Cancel();
            }
        }

        public void Close()
        {
            Log.Debug("Close requested");
            if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
            {
                Log.Debug("Cancellation token triggered");
                cancellationTokenSource.Cancel();
            }
        }

        private void SocketListener()
        {
            Log.Debug("SocketListener started");

            var endPoint = new IPEndPoint(IPAddress.Any, 9123);
            using (var listener = new UdpClient(endPoint))
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    listener.Client.ReceiveTimeout = 1000;
                    try
                    {
                        var bytes = listener.Receive(ref remoteEndPoint);
                        var msg = string.Format("Recieved {0} bytes from {1}", bytes.Length, remoteEndPoint.Address);
                        Log.Debug(msg);
                        Trace.WriteLine(msg);

                        var message = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                        lock (PendingQueue)
                        {
                            PendingQueue.Enqueue(message);
                        }
                    }
                    catch (Exception e)
                    {
                        Log.DebugFormat("SocketException: {0}", e.Message);
                        Trace.WriteLine(string.Format("SocketException: {0}", e.Message));
                    }
                }
            }

            Log.Debug("SocketListener completed");
        }

        private void MessagePump()
        {
            Log.Debug("MessagePump started");

            while (!cancellationTokenSource.IsCancellationRequested)
            {
                Thread.Sleep(PumpFrequency);

                try
                {
                    if (Logger != null)
                    {
                        lock (PendingQueue)
                        {
                            var processedQueue = new Queue<ILogEntry>();

                            while (PendingQueue.Count > 0)
                            {
                                var message = PendingQueue.Dequeue();

                                // TODO: validate
                                if (IsValidMessage(message))
                                {
                                    var deserializeMessage = DeserializeMessage(message);

                                    if (deserializeMessage != null)
                                    {
                                        processedQueue.Enqueue(deserializeMessage);
                                    }
                                }
                            }

                            if (processedQueue.Any())
                            {
                                Logger.AddBatch(processedQueue);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            Log.Debug("MessagePump completed");
        }

        private ILogEntry DeserializeMessage(string message)
        {
            LogEntry logEntry;

            try
            {
                XNamespace log4Net = "unique";

                var payload = string.Format(@"<entry xmlns:log4net=""{0}"">{1}</entry>", log4Net, message);
                var element = XElement.Parse(payload);
                var record = element.Element(log4Net + "event");

                // Establish whether a sub-system seems to be defined.
                var description = record.Element(log4Net + "message").Value;

                var classification = string.Empty;
                var system = record.Attribute("logger").Value;
                var type = record.Attribute("level").Value;
                var host = "???";

                foreach (XElement propertyElement in record.Element(log4Net + "properties").Elements())
                {
                    if (propertyElement.Name == log4Net + "data" && propertyElement.Attribute("name") != null)
                    {
                        host = propertyElement.Attribute("value").Value;
                    }
                }

                logEntry = new LogEntry
                               {
                                   DateTime = DateTime.Parse(record.Attribute("timestamp").Value),
                                   System = system,
                                   Thread = record.Attribute("thread").Value,
                                   Description = description,
                                   Type = type,
                                   MetaData = new Dictionary<string, object>
                                                  {
                                                      {
                                                          "Classification", classification
                                                          },
                                                      {
                                                          "Host", host
                                                          }
                                                  }
                               };
            }
            catch (Exception e)
            {
                return null;
            }

            return logEntry;
        }

        private bool IsValidMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentNullException("message");
            }

            return message.StartsWith("<log4net:event");
        }
    }
}
