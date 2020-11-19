namespace Sentinel.Interfaces.Providers
{
    using System;
    using System.Runtime.Serialization;

    public interface IProviderInfo
    {
        [DataMember]
        Guid Identifier { get; }

        [DataMember]
        string Name { get; }

        [DataMember]
        string Description { get; }
    }
}