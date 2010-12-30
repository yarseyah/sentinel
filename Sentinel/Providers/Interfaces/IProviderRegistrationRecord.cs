using System;
using Sentinel.Providers.Interfaces;

namespace Sentinel.Providers.Interfaces
{
    public interface IProviderRegistrationRecord
    {
        Guid Identifier { get; }
        IProviderInfo Info { get; }
        Type Settings { get; }
        Type Implementor { get; }
    }
}