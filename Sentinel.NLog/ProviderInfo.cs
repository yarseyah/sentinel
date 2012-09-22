namespace Sentinel.NLog
{
    using System;

    using Sentinel.Interfaces.Providers;

    public class ProviderInfo : IProviderInfo
    {
        public ProviderInfo(Guid uniqueId, string name, string description)
        {
            Identifier = uniqueId;
            Name = name;
            Description = description;
        }

        public Guid Identifier { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }
    }
}