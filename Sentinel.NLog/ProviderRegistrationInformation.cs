namespace Sentinel.NLog
{
    using System;

    using Interfaces.Providers;

    public class ProviderRegistrationInformation : IProviderRegistrationRecord
    {
        public ProviderRegistrationInformation(IProviderInfo providerInfo)
        {
            Info = providerInfo;
        }

        public Guid Identifier => Info.Identifier;

        public IProviderInfo Info { get; private set; }

        public Type Settings => typeof(NetworkConfigurationPage);

        public Type Implementor => typeof(NLogViewerProvider);
    }
}