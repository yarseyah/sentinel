using System.Runtime.Serialization;
namespace Sentinel.NLog
{
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
                    "Listens on {0} port {1}",
                    Protocol,
                    Port);
            }
        }
    }
}