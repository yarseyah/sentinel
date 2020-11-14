namespace Sentinel.Interfaces.Providers
{
    using System;

    public interface IProviderRegistrationRecord
    {
        Guid Identifier { get; }

        IProviderInfo Info { get; }

        Type Settings { get; }

        Type Implementer { get; }
    }
}