using System;
using Sentinel.Providers.Interfaces;

namespace Sentinel.Providers
{
    public class ProviderRegistrationRecord : IProviderRegistrationRecord
    {
        public Guid Identifier { get; set; }
        public ProviderInfo Info { get; set; }
        public Type Settings { get; set; }
        public Type Implementor { get; set; }
    }
}
