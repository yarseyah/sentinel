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
    using Sentinel.Interfaces.CodeContracts;
    using Sentinel.Interfaces.Providers;

    public class Log4NetProvider : INetworkProvider
    {
        public static readonly IProviderRegistrationRecord ProviderRegistrationInformation =
            new ProviderRegistrationInformation(new ProviderInfo());

        private const int PumpFrequency = 100;

        private static readonly XmlNamespaceManager NamespaceManager = new XmlNamespaceManager(new NameTable());

        private static readonly ILog Log = LogManager.GetLogger<Log4NetProvider>();

        private readonly Queue<string> pendingQueue = new Queue<string>();

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
            settings.ThrowIfNull(nameof(settings));

            udpSettings = settings as IUdpAppenderListenerSettings;
            udpSettings.ThrowIfNull(nameof(udpSettings));

            Information = ProviderRegistrationInformation.Info;
            ProviderSettings = udpSettings;
        }

        public IProviderInfo Information { get; private set; }

        public IProviderSettings ProviderSettings { get; private set; }

        public ILogger Logger { get; set; }

        public string Name { get; set; }

        public bool IsActive => udpListenerTask != null && udpListenerTask.Status == TaskStatus.Running;

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
                Log.Error("UDP settings has not been initialized");
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

                            Log.Debug($"Received {bytes.Length} bytes from {remoteEndPoint.Address}");

                            var message = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                            lock (pendingQueue)
                            {
                                pendingQueue.Enqueue(message);
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
                        lock (pendingQueue)
                        {
                            var processedQueue = new Queue<ILogEntry>();

                            while (pendingQueue.Count > 0)
                            {
                                var message = pendingQueue.Dequeue();

                                // TODO: validate
                                if (IsValidMessage(message))
                                {
                                    var deserializedMessage = DeserializeMessage(message);

                                    if (deserializedMessage != null)
                                    {
                                        processedQueue.Enqueue(deserializedMessage);
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
                    Log.Error("Unspecific exception", e);
                }
            }

            Log.Debug("MessagePump completed");
        }

        private ILogEntry DeserializeMessage(string message)
        {
            try
            {
                // Record the current date/time
                var receivedTime = DateTime.UtcNow;

                var payload = $@"<entry xmlns:log4net=""{log4Net}"">{message}</entry>";
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
                    var props = new Dictionary<string, object>();
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
                                    if (props.ContainsKey(name))
                                    {
                                        props[name] = value;
                                    }
                                    else
                                    {
                                        props.Add(name, value);
                                    }

                                    break;
                            }
                        }
                    }

                    var className = string.Empty;
                    var methodName = string.Empty;
                    var sourceFile = string.Empty;
                    var line = string.Empty;

                    // Any source information
                    var source = entryEvent.Element(log4Net + "locationInfo");
                    if (source != null)
                    {
                        className = source.Attribute("class").Value;
                        methodName = source.Attribute("method").Value;
                        sourceFile = source.Attribute("file").Value;
                        line = source.Attribute("line").Value;
                    }

                    var metaData = new Dictionary<string, object>
                    {
                        ["Classification"] = classification,
                        ["Host"] = host,
                    };

                    foreach (var prop in props)
                    {
                        if (metaData.ContainsKey(prop.Key))
                        {
                            Log.Warn($"Already have property of {prop.Key}, overwriting");
                            metaData[prop.Key] = prop.Value;
                        }
                        else
                        {
                            metaData.Add(prop.Key, prop.Value);
                        }
                    }

                    AddExceptionIfFound(entryEvent, metaData);

                    // Extract from the source the originating date/time
                    var sourceTime = entryEvent.GetAttributeDateTime("timestamp", DateTime.Now);

                    var logEntry = new LogEntry
                                       {
                                           DateTime = sourceTime,
                                           System = system,
                                           Thread = entryEvent.GetAttribute("thread", string.Empty),
                                           Description = description,
                                           Type = type,
                                           MetaData = metaData,
                                       };

                    if (logEntry.Description.ToUpper().Contains("EXCEPTION"))
                    {
                        logEntry.MetaData.Add("Exception", true);
                    }

                    if (!string.IsNullOrWhiteSpace(className))
                    {
                        // TODO: use an object for these?
                        logEntry.MetaData.Add("ClassName", className);
                        logEntry.MetaData.Add("MethodName", methodName);
                        logEntry.MetaData.Add("SourceFile", sourceFile);
                        logEntry.MetaData.Add("SourceLine", line);
                    }

                    logEntry.MetaData.Add("ReceivedTime", receivedTime);

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
            entryEvent.ThrowIfNull(nameof(entryEvent));

            metaData.ThrowIfNull(nameof(metaData));

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
                throw new ArgumentNullException(nameof(message));
            }

            return message.StartsWith("<log4net:event");
        }

        internal class ProviderInfo : IProviderInfo
        {
            public Guid Identifier => new Guid("D19E8097-FC08-47AF-8418-F737168A9645");

            public string Name => "Log4Net UdpAppender Provider";

            public string Description => "Handler for the remote side of log4net's UdpAppender";
        }
    }
}