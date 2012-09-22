namespace Sentinel.NLog
{
    public class NetworkSettings : ProviderSettings
    {
        public NetworkSettings()
        {
            Port = 9999;
            IsUdp = true;
        }

        public bool IsUdp
        {
            get;
            set;
        }

        public int Port
        {
            get;
            set;
        }

        public override string Summary
        {
            get
            {
                return string.Format(
                    "Listens on {0} port {1}",
                    IsUdp ? "UDP" : "TCP",
                    Port);
            }
        }
    }
}