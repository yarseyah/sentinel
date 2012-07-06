namespace Sentinel.Log4Net
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    using Common.Logging;

    using Sentinel.Interfaces;
    using Sentinel.Interfaces.Providers;

    public class UdpAppenderListener : INetworkProvider
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public static readonly IProviderRegistrationRecord ProviderRegistrationInformation =
            new ProviderRegistrationInformation(new Log4NetUdpListenerProvider());

        private readonly IUdpAppenderListenerSettings udpSettings;

        private CancellationTokenSource cancellationTokenSource;

        private Task udpListenerTask;

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

        public IProviderInfo Information
        {
            get;
            private set;
        }

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

            if ( udpListenerTask == null || udpListenerTask.IsCompleted )
            {
                cancellationTokenSource = new CancellationTokenSource();
                var token = cancellationTokenSource.Token;

                udpListenerTask = Task.Factory.StartNew(Worker, token);
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

        private void Worker()
        {
            Log.Debug("Worker started");

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
                    }
                    catch (Exception e)
                    {
                        Log.DebugFormat("SocketException: {0}", e.Message);
                        Trace.WriteLine(string.Format("SocketException: {0}", e.Message));
                    }
                }
            }

            Log.Debug("Worker completed");
        }
    }
}
