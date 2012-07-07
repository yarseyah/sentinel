namespace Sentinel.Providers.Interfaces
{
    using System;
    using System.Collections.Generic;

    using Sentinel.Interfaces.Providers;

    public interface IProviderManager : IEnumerable<Guid>
    {
        void Register(IProviderRegistrationRecord record);

        ILogProvider Create(Guid providerGuid, IProviderSettings settings);

        ILogProvider Get(string name);

        void Remove(string name);

        IEnumerable<Guid> GetRegistered();

        IProviderInfo GetInformation(Guid providerGuid);

        T GetConfiguration<T>(Guid providerGuid);

        IEnumerable<ILogProvider> GetInstances();
    }
}