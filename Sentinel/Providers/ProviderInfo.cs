using System;
using ProtoBuf;

namespace Sentinel.Providers
{
    [ProtoContract]
    public class ProviderInfo
    {
        public ProviderInfo(Guid uniqueId, string name, string description)
        {
            Identifier = uniqueId;
            Name = name;
            Description = description;
        }

        [ProtoMember(1)]
        public Guid Identifier { get; private set; }

        [ProtoMember(2)]
        public string Name { get; private set; }

        [ProtoMember(3)]
        public string Description { get; private set; }
    }
}