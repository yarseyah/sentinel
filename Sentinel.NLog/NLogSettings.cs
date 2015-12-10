namespace Sentinel.NLog
{
    using System.Runtime.Serialization;

    [DataContract]
    public class NetworkSettings : ProviderSettings, INLogAppenderSettings
    {
        public NetworkSettings()
        {
            Port = 9999;
            Protocol = NetworkProtocol.Udp;
        }

        public NetworkProtocol Protocol { get; set; }

        public int Port { get; set; }

        public override string Summary
        {
            get
            {
                return $"{Name}: Listens on {Protocol} port {Port}";
            }
        }
    }
}