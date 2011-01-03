using System;
using System.Collections.Generic;

namespace Sentinel.Providers.Interfaces
{
    public interface IProviderManager : IEnumerable<Guid>
    {
        void Register(IProviderRegistrationRecord record);
        ILogProvider Create(Guid providerGuid, IProviderSettings settings);
        ILogProvider Get(string name);
        void Remove(string name);

        IEnumerable<Guid> GetRegistered();
        ProviderInfo GetInformation(Guid providerGuid);

        T GetConfiguration<T>(Guid providerGuid);

        IEnumerable<ILogProvider> GetInstances();
    }
}