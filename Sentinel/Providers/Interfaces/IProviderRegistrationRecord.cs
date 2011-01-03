using System;

namespace Sentinel.Providers.Interfaces
{
    public interface IProviderRegistrationRecord
    {
        Guid Identifier { get; }
        ProviderInfo Info { get; }
        Type Settings { get; }
        Type Implementor { get; }
    }
}