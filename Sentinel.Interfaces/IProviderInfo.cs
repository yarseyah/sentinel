namespace Sentinel.Interfaces
{
    using System;

    public interface IProviderInfo
    {
        Guid Identifier { get; }

        string Name { get; }

        string Description { get; }
    }
}