using System;
using Sentinel.Providers.Interfaces;

namespace Sentinel.Providers
{
    public class ProviderInfo : IProviderInfo
    {
        public ProviderInfo(Guid uniqueId, string name, string description)
        {
            Identifier = uniqueId;
            Name = name;
            Description = description;
        }

        #region Implementation of IProviderInfo

        public Guid Identifier { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }

        #endregion
    }
}