namespace Sentinel.Providers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using Sentinel.FileMonitor;
    using Sentinel.Interfaces;
    using Sentinel.Interfaces.CodeContracts;
    using Sentinel.Interfaces.Providers;
    using Sentinel.Log4Net;
    using Sentinel.MSBuild;
    using Sentinel.NLog;
    using Sentinel.Providers.Interfaces;

    public class ProviderManager : IProviderManager
    {
        private readonly IList<IProviderRegistrationRecord> providers;

        private readonly List<KeyValuePair<string, ILogProvider>> providerInstances =
            new List<KeyValuePair<string, ILogProvider>>();

        public ProviderManager()
        {
            providers = new List<IProviderRegistrationRecord>
                            {
                                NLogViewerProvider.ProviderRegistrationInformation,
                                Log4NetProvider.ProviderRegistrationInformation,
                                FileMonitoringProvider.ProviderRegistrationInformation,
                                MsBuildProvider.ProviderRegistrationRecord,
                            };
        }

        public IEnumerable<Guid> Registered => providers.Select(p => p.Identifier);

        public IEnumerable<ILogProvider> Instances => providerInstances.Select(i => i.Value);

        public void Register(IProviderRegistrationRecord record)
        {
            throw new NotImplementedException("Dynamic registration is not yet supported");
        }

        public ILogProvider Create(Guid providerGuid, IProviderSettings settings)
        {
            settings.ThrowIfNull(nameof(settings));

            // Make sure we don't have any instances of that providerGuid.
            if (providerInstances.Any(p => p.Key == settings.Name && p.Value.Information.Identifier == providerGuid))
            {
                throw new ArgumentException(
                    "Already an instance of that ILoggerProvider with that name specified",
                    nameof(settings));
            }

            // Make sure that the type is supported.))
            if (providers.All(p => p.Identifier != providerGuid))
            {
                Trace.WriteLine("No provider with the identifier " + providerGuid + " is registered");
                return null;
            }

            // Get an instance.
            var record = providers.FirstOrDefault(p => p.Identifier == providerGuid);

            if (record != null)
            {
                Debug.Assert(record.Implementer != null, "Need to know the implementing type for the provider");

                try
                {
                    var provider = (ILogProvider)Activator.CreateInstance(record.Implementer, settings);
                    providerInstances.Add(new KeyValuePair<string, ILogProvider>(settings.Name, provider));
                    return provider;
                }
                catch (Exception e)
                {
                    Trace.TraceError(e.ToString());
                    Debugger.Break();
                }
            }

            return null;
        }

        public ILogProvider Get(string name)
        {
            Debug.Assert(providerInstances.Any(p => p.Key == name), "There is no instance with the identifier " + name);
            if (providerInstances.All(p => p.Key != name))
            {
                throw new ArgumentException("There is no instance with the identifier " + name, nameof(name));
            }

            return providerInstances.FirstOrDefault(p => p.Key == name).Value;
        }

        public void Remove(string name)
        {
            throw new NotSupportedException("Removal is not yet supported");
        }

        public IProviderInfo GetInformation(Guid providerGuid)
        {
            Debug.Assert(providers.Any(p => p.Identifier == providerGuid), "No such registered Provider");
            if (providers.All(p => p.Identifier != providerGuid))
            {
                throw new ArgumentException(
                    "Specified guid does not correspond to a registered provider",
                    nameof(providerGuid));
            }

            return providers.First(p => p.Identifier == providerGuid).Info;
        }

        /// <summary>
        /// Gets the configuration abstraction for the specified Guid.
        /// Type left to caller to determine (as long as it is satisfied
        /// by the implementer too).
        /// </summary>
        /// <typeparam name="T">Type of configuration.</typeparam>
        /// <param name="providerGuid">Identifier of provider.</param>
        /// <returns>Returns the provider associated to the supplied <see cref="Guid"/>>.</returns>
        public T GetConfiguration<T>(Guid providerGuid)
        {
            var matchesGuid = providers.Where(p => p.Identifier == providerGuid).Where(p => p.Settings != null);

            // Simple checking for duplications.  At the moment, throw an
            // exception if it happens - in the future, it might change to
            // a last-registered wins policy (or maybe first wins!)
            var matchesType = matchesGuid.Count(p => p.Settings.GetInterfaces().Any(i => i.IsAssignableFrom(typeof(T))));

            if (matchesType > 1)
            {
                throw new NotSupportedException(
                    $"There should only be one registered {typeof(T)} handler for the provider {providerGuid}");
            }

            var matches =
                matchesGuid.Where(p => p.Settings.GetInterfaces().Any(i => i.IsAssignableFrom(typeof(T)))).Select(
                    p => p.Settings);

            var match = matches.LastOrDefault();
            if (match != null)
            {
                return (T)Activator.CreateInstance(match);
            }

            return default(T);
        }

        public IEnumerator<Guid> GetEnumerator()
        {
            return providers.Select(p => p.Identifier).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
