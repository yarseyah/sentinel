namespace Sentinel.Log4Net
{
    using System.Threading;
    using System.Threading.Tasks;

    using Common.Logging;

    using Sentinel.Interfaces;
    using Sentinel.Interfaces.Providers;

    public class UdpAppenderListener : INetworkProvider
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private CancellationTokenSource cancellationTokenSource;

        private Task udpListenerTask;

        public UdpAppenderListener(IUdpAppenderListenerSettings setting)
        {
            Information = new Log4NetUdpListenerProvider();
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

            while (!cancellationTokenSource.IsCancellationRequested)
            {
                Log.Debug("Ping...");
                Thread.Sleep(1000);
            }

            Log.Debug("Worker completed");
        }
    }
}
