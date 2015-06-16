namespace Sentinel.MSBuild
{
    using System;

    using Sentinel.Interfaces.Providers;

    public class ProviderRegistrationInformation : IProviderRegistrationRecord
    {
        public ProviderRegistrationInformation(IProviderInfo providerInfo)
        {
            Info = providerInfo;
        }

        public Guid Identifier
        {
            get
            {
                return Info.Identifier;
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
                return typeof(MSBuildProvider);
            }
        }
    }
}