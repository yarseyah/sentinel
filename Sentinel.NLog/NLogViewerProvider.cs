namespace Sentinel.NLog
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    using Common.Logging;

    using Sentinel.Interfaces;
    using Sentinel.Interfaces.CodeContracts;
    using Sentinel.Interfaces.Providers;

    public class NLogViewerProvider : INetworkProvider
    {
        public static readonly IProviderRegistrationRecord ProviderRegistrationInformation
            = new ProviderRegistrationInformation(new ProviderInfo());

        private const int PumpFrequency = 100;

        private static readonly DateTime Log4JDateBase = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static readonly ILog Log = LogManager.GetLogger<NLogViewerProvider>();

        private readonly Queue<string> pendingQueue = new Queue<string>();

        private readonly INLogAppenderSettings networkSettings;

        private CancellationTokenSource cancellationTokenSource;

        private Task listenerTask;

        public NLogViewerProvider(IProviderSettings settings)
        {
            settings.ThrowIfNull(nameof(settings));

            networkSettings = settings as INLogAppenderSettings;
            networkSettings.ThrowIfNull(nameof(networkSettings));

            Information = ProviderRegistrationInformation.Info;
            ProviderSettings = networkSettings;
        }

        public IProviderInfo Information { get; private set; }

        public IProviderSettings ProviderSettings { get; private set; }

        public ILogger Logger { get; set; }

        public string Name { get; set; }

        public bool IsActive => listenerTask != null && listenerTask.Status == TaskStatus.Running;

        public int Port { get; private set; }

        public void Start()
        {
            Log.Debug("Start requested");

            if (listenerTask == null || listenerTask.IsCompleted)
            {
                cancellationTokenSource = new CancellationTokenSource();
                var token = cancellationTokenSource.Token;

                listenerTask = Task.Factory.StartNew(SocketListener, token);
                Task.Factory.StartNew(MessagePump, token);
            }
            else
            {
                Log.WarnFormat(
                    "{0} listener task is already active and can not be started again.",
                    networkSettings.Protocol);
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

            if (networkSettings == null)
            {
                Log.Error("Network settings has not been initialized");
                throw new NullReferenceException();
            }

            while (!cancellationTokenSource.IsCancellationRequested)
            {
                var endPoint = new IPEndPoint(IPAddress.Any, networkSettings.Port);

                using (var listener = new NetworkClientWrapper(networkSettings.Protocol, endPoint))
                {
                    while (!cancellationTokenSource.IsCancellationRequested)
                    {
                        var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

                        listener.SetReceiveTimeout(1000);

                        try
                        {
                            var bytes = listener.Receive(ref remoteEndPoint);

                            Log.DebugFormat(
                                "Received {0} bytes from {1} ({2})",
                                bytes.Length,
                                remoteEndPoint.Address,
                                networkSettings.Protocol);

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

                                if (IsValidEntry(message))
                                {
                                    var deserializeMessage = DecodeEntry(message);

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

        private bool IsValidEntry(string logEntry)
        {
            return logEntry.StartsWith("<log4j");
        }

        private LogEntry DecodeEntry(string m)
        {
            // Record the current date/time
            var receivedTime = DateTime.UtcNow;

            XNamespace log4J = "unique";
            XNamespace nlogNamespace = "nlogUnique";
            var message = $@"<entry xmlns:log4j=""{log4J}"" xmlns:nlog=""{nlogNamespace}"">{m}</entry>";
            var element = XElement.Parse(message);
            var record = element.Element(log4J + "event");

            // Establish whether a sub-system seems to be defined.
            var description = record.Element(log4J + "message").Value;

            var classification = string.Empty;
            var system = record.Attribute("logger")?.Value ?? string.Empty;
            var type = record.Attribute("level")?.Value ?? string.Empty;
            var host = "Unknown";

            var meta = new Dictionary<string, object>();

            foreach (var propertyElement in record.Element(log4J + "properties")?.Elements()
                                            ?? Enumerable.Empty<XElement>())
            {
                if (propertyElement.Name == log4J + "data")
                {
                    var name = propertyElement.Attribute("name")?.Value;
                    var value = propertyElement.Attribute("value")?.Value;

                    if (name == "log4jmachinename")
                    {
                        host = value;
                    }
                    else if (!string.IsNullOrWhiteSpace(name))
                    {
                        if (meta.ContainsKey(name))
                        {
                            Log.Warn($"Already have property of {name}, overwriting");
                        }

                        meta.Add(name, value);
                    }
                }
            }

            var source = record.Element(log4J + "locationInfo");

            // Any source information
            var className = source?.Attribute("class")?.Value ?? string.Empty;
            var methodName = source?.Attribute("method")?.Value ?? string.Empty;
            var sourceFile = source?.Attribute("file")?.Value ?? string.Empty;
            var line = source?.Attribute("line")?.Value ?? string.Empty;

            var timestampValue = record.Attribute("timestamp")?.Value;
            var date = DateTime.UtcNow;
            if (timestampValue != null)
            {
                var timestamp = double.Parse(timestampValue);
                date = Log4JDateBase + TimeSpan.FromMilliseconds(timestamp);
            }

            meta["Classification"] = classification;
            meta["Host"] = host;

            var entry = new LogEntry
                            {
                                DateTime = date,
                                System = system,
                                Thread = record.Attribute("thread")?.Value ?? string.Empty,
                                Description = description,
                                Type = type,
                                MetaData = meta,
                            };
            if (entry.Description.ToUpper().Contains("EXCEPTION"))
            {
                entry.MetaData.Add("Exception", true);
            }

            if (!string.IsNullOrWhiteSpace(className))
            {
                // TODO: use an object for these?
                meta.Add("ClassName", className);
                meta.Add("MethodName", methodName);
                meta.Add("SourceFile", sourceFile);
                meta.Add("SourceLine", line);
            }

            meta.Add("ReceivedTime", receivedTime);

            return entry;
        }
    }
}