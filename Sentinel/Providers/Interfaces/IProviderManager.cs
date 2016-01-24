namespace Sentinel.Providers.Interfaces
{
    using System;
    using System.Collections.Generic;

    using Sentinel.Interfaces.Providers;

    public interface IProviderManager : IEnumerable<Guid>
    {
        IEnumerable<ILogProvider> Instances { get; }

        IEnumerable<Guid> Registered { get; }

        void Register(IProviderRegistrationRecord record);

        ILogProvider Create(Guid providerGuid, IProviderSettings settings);

        ILogProvider Get(string name);

        void Remove(string name);

        IProviderInfo GetInformation(Guid providerGuid);

        T GetConfiguration<T>(Guid providerGuid);
    }
}