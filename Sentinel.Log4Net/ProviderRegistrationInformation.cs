namespace Sentinel.Log4Net
{
    using System;

    using Sentinel.Interfaces.Providers;

    public class ProviderRegistrationInformation : IProviderRegistrationRecord
    {
        public ProviderRegistrationInformation(IProviderInfo providerInfo)
        {
            this.Info = providerInfo;
        }

        public Guid Identifier
        {
            get
            {
                return this.Info.Identifier;
            }
        }

        public IProviderInfo Info { get; private set; }

        public Type Settings
        {
            get
            {
                return typeof(ConfigurationPage);
            }
        }

        public Type Implementor
        {
            get
            {
                return typeof(UdpAppenderListener);
            }
        }
    }
}