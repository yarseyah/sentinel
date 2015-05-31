namespace Sentinel.Log4Net
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Linq;

    using Common.Logging;

    using Sentinel.Interfaces;
    using Sentinel.Interfaces.Providers;

    public class Log4NetProvider : INetworkProvider
    {
        public static readonly IProviderRegistrationRecord ProviderRegistrationInformation =
            new ProviderRegistrationInformation(new ProviderInfo());

        protected readonly Queue<string> PendingQueue = new Queue<string>();

        private const int PumpFrequency = 100;

        private static readonly XmlNamespaceManager NamespaceManager = new XmlNamespaceManager(new NameTable());

        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private readonly IUdpAppenderListenerSettings udpSettings;

        private readonly XNamespace log4Net = "unique";

        private CancellationTokenSource cancellationTokenSource;

        private Task udpListenerTask;

        private Task messagePumpTask;

        static Log4NetProvider()
        {
            NamespaceManager.AddNamespace("log4net", "http://logging.apache.org/log4net");
        }

        public Log4NetProvider(IProviderSettings settings)
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
            ProviderSettings = udpSettings;
        }

        public IProviderInfo Information { get; private set; }

        public IProviderSettings ProviderSettings { get; private set; }

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

            if (udpSettings == null)
            {
                Log.Error("UDP settings has not been initialised");
                throw new NullReferenceException();
            }

            while (!cancellationTokenSource.IsCancellationRequested)
            {
                var endPoint = new IPEndPoint(IPAddress.Any, udpSettings.Port);

                using (var listener = new UdpClient(endPoint))
                {
                    while (!cancellationTokenSource.IsCancellationRequested)
                    {
                        var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                        listener.Client.ReceiveTimeout = 1000;
                        try
                        {
                            var bytes = listener.Receive(ref remoteEndPoint);

                            Log.Debug(string.Format("Received {0} bytes from {1}", bytes.Length, remoteEndPoint.Address));

                            var message = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                            lock (PendingQueue)
                            {
                                PendingQueue.Enqueue(message);
                            }
                        }
                        catch (SocketException socketException)
                        {
                            if (socketException.SocketErrorCode != SocketError.TimedOut)
                            {
                                Log.Debug("SocketException", socketException);
                                Log.DebugFormat(
                                    "SocketException.SocketErrorCode = {0}", 
                                    socketException.SocketErrorCode);

                                // Break out of the 'using socket' loop and try to establish a new socket.
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                            Log.DebugFormat("Exception: {0}", e.Message);
                        }
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
            
            try
            {
                var payload = string.Format(@"<entry xmlns:log4net=""{0}"">{1}</entry>", log4Net, message);
                var element = XElement.Parse(payload);
                var entryEvent = element.Element(log4Net + "event");

                // Establish whether a sub-system seems to be defined.
                if (entryEvent != null)
                {
                    var description = entryEvent.Element(log4Net + "message").Value;

                    var classification = string.Empty;
                    var system = entryEvent.GetAttribute("logger", string.Empty);
                    var type = entryEvent.GetAttribute("level", string.Empty);
                    var host = string.Empty;

                    foreach (var propertyElement in entryEvent.Element(log4Net + "properties").Elements())
                    {
                        if (propertyElement.Name == log4Net + "data")
                        {
                            var name = propertyElement.GetAttribute("name", string.Empty);
                            var value = propertyElement.GetAttribute("value", string.Empty);

                            switch (name)
                            {
                                case "log4net:HostName":
                                    host = value;
                                    break;
                                default:
                                    Log.WarnFormat("Found unknown property named '{0}' with value '{1}'", name, value);
                                    break;
                            }
                        }
                    }

                    var metaData = new Dictionary<string, object>();
                    metaData["Classification"] = classification;
                    metaData["Host"] = host;

                    AddExceptionIfFound(entryEvent, metaData);

                    var logEntry = new LogEntry
                        {
                            DateTime = entryEvent.GetAttributeDateTime("timestamp", DateTime.Now),
                            System = system,
                            Thread = entryEvent.GetAttribute("thread", string.Empty),
                            Description = description,
                            Type = type,
                            MetaData = metaData
                        };

                    return logEntry;
                }
            }
            catch (Exception e)
            {
                Log.Error("DeserializeMessage: exception when processing incoming message", e);
            }

            return null;
        }

        private void AddExceptionIfFound(XElement entryEvent, Dictionary<string, object> metaData)
        {
            if (entryEvent == null)
            {
                throw new ArgumentNullException("entryEvent");
            }

            if (metaData == null)
            {
                throw new ArgumentNullException("metaData");
            }

            var exceptionElement = entryEvent.Element(log4Net + "exception");
            if (exceptionElement != null)
            {
                metaData["Exception"] = exceptionElement.Value;
            }
        }

        private bool IsValidMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentNullException("message");
            }

            return message.StartsWith("<log4net:event");
        }

        internal class ProviderInfo : IProviderInfo
        {
            public Guid Identifier
            {
                get
                {
                    return new Guid("D19E8097-FC08-47AF-8418-F737168A9645");
                }
            }

            public string Name
            {
                get
                {
                    return "Log4Net UdpAppender Provider";
                }
            }

            public string Description
            {
                get
                {
                    return "Handler for the remote side of log4net's UdpAppender";
                }
            }
        }
    }
}
