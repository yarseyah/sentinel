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
                return string.Format(
                    "{0}: Listens on {1} port {2}",
                    Name,
                    Protocol,
                    Port);
            }
        }
    }
}