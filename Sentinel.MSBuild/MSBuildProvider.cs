namespace Sentinel.MSBuild
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Common.Logging;

    using Newtonsoft.Json.Linq;

    using Sentinel.Interfaces;
    using Sentinel.Interfaces.CodeContracts;
    using Sentinel.Interfaces.Providers;

    public class MsBuildProvider : INetworkProvider
    {
        public static readonly IProviderRegistrationRecord ProviderRegistrationRecord =
            new ProviderRegistrationInformation(new ProviderInfo());

        private const int PumpFrequency = 100;

        private static readonly ILog Log = LogManager.GetLogger<MsBuildProvider>();

        private readonly Queue<string> pendingQueue = new Queue<string>();

        private CancellationTokenSource cancellationTokenSource;

        private Task listenerTask;

        public MsBuildProvider(IProviderSettings settings)
        {
            settings.ThrowIfNull(nameof(settings));

            Settings = settings as IMsBuildListenerSettings;
            Settings.ThrowIfNull(nameof(Settings));

            ProviderSettings = settings;
        }

        public IProviderInfo Information { get; private set; }

        public IProviderSettings ProviderSettings { get; private set; }

        public ILogger Logger { get; set; }

        public string Name { get; set; }

        public bool IsActive => listenerTask != null && listenerTask.Status == TaskStatus.Running;

        public int Port { get; private set; }

        protected IMsBuildListenerSettings Settings { get; set; }

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

            while (!cancellationTokenSource.IsCancellationRequested)
            {
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
                                Log.Debug($"SocketException.SocketErrorCode = {socketException.SocketErrorCode}");

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
                var json = JToken.Parse(message);

                var jsonObject = json as JObject;

                if (jsonObject != null && jsonObject.Children().Count() == 1)
                {
                    var property = jsonObject.Children().First() as JProperty;

                    if (property == null)
                    {
                        Log.Error("First item in JObject should be a property");
                    }
                    else
                    {
                        var msbuildEventType = property.Name;
                        var content = property.Value as JObject;

                        if (string.IsNullOrWhiteSpace(msbuildEventType) || content == null)
                        {
                            Log.ErrorFormat(
                                "Expected payload to consist of a property corresponding to the MSBuild event type name, "
                                + "and a value which is the serialized object corresponding to the type.");
                        }
                        else
                        {
                            return new LogEntry(msbuildEventType, content);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Deserialization exception trying to turn the JSON content into a LogMessage", e);
            }

            return null;
        }

        private bool IsValidMessage(string message)
        {
            // TODO: validation logic required.
            return true;
        }
    }
}