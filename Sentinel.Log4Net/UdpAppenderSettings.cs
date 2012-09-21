namespace Sentinel.Log4Net
{
    using Sentinel.Interfaces.Providers;

    public class UdpAppenderSettings : IUdpAppenderListenerSettings
    {
        public UdpAppenderSettings(IProviderSettings providerSettings)
        {
            ProviderSettings = providerSettings;
        }

        public string Name
        {
            get
            {
                return ProviderSettings.Name;
            }
        }

        public string Summary
        {
            get
            {
                return ProviderSettings.Summary;
            }
        }

        public IProviderInfo Info
        {
            get
            {
                return ProviderSettings.Info;
            }
        }

        public int Port
        {
            get; 
            set;
        }

        private IProviderSettings ProviderSettings { get; set; }
    }
}