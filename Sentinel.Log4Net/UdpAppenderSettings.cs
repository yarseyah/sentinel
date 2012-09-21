namespace Sentinel.Log4Net
{
    using Sentinel.Interfaces.Providers;

    public class UdpAppenderSettings : IUdpAppenderListenerSettings
    {
        public UdpAppenderSettings()
        {
            Name = "Log4net UDP Appender";
            Info = UdpAppenderListener.ProviderRegistrationInformation.Info;
        }

        public UdpAppenderSettings(IProviderSettings providerInfo)
        {
            Name = providerInfo.Name;
            Info = providerInfo.Info;
            Summary = providerInfo.Summary;
        }

        public string Name { get; set; }

        public string Summary { get; set; }

        public IProviderInfo Info { get; set; }

        public int Port
        {
            get; 
            set;
        }
    }
}