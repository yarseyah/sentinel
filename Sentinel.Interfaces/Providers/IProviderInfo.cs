namespace Sentinel.Interfaces.Providers
{
    using System;

    public interface IProviderInfo
    {
        Guid Identifier { get; }

        string Name { get; }

        string Description { get; }
    }
}