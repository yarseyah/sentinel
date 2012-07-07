
namespace Sentinel.Providers
{
    using System;

    using Sentinel.Interfaces.Providers;

    public class ProviderRegistrationRecord : IProviderRegistrationRecord
    {
        public Guid Identifier { get; set; }

        public IProviderInfo Info { get; set; }

        public Type Settings { get; set; }

        public Type Implementor { get; set; }
    }
}
