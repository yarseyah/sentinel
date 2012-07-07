namespace Sentinel.Log4Net
{
    using Sentinel.Interfaces.Providers;

    public class UdpAppenderSettings : IUdpAppenderListenerSettings
    {
        public UdpAppenderSettings(IProviderSettings previousSettings)
        {
            PreviousSettings = previousSettings;
        }

        public string Name
        {
            get
            {
                return PreviousSettings.Name;
            }
        }

        public string Summary
        {
            get
            {
                return PreviousSettings.Summary;
            }
        }

        /// <summary>
        /// Reference back to the provider this setting is appropriate to.
        /// </summary>
        public IProviderInfo Info
        {
            get
            {
                return PreviousSettings.Info;
            }
        }

        private IProviderSettings PreviousSettings { get; set; }
    }
}