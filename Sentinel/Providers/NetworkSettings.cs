using Sentinel.Providers.Interfaces;

namespace Sentinel.Providers
{
    public class NetworkSettings : ProviderSettings, INetworkProviderSettings
    {
        public NetworkSettings()
        {
            Port = 9999;
            IsUdp = true;
        }

        public int Port
        {
            get;
            set;
        }

        public bool IsUdp
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