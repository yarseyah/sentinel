namespace Sentinel.Providers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

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
                                new ProviderRegistrationRecord
                                    {
                                        Identifier = NLogViewerProvider.Id,
                                        Info = NLogViewerProvider.Info,
                                        Implementor = typeof(NLogViewerProvider),
                                        Settings = typeof(NetworkConfigurationPage)
                                    },
                                new ProviderRegistrationRecord
                                    {
                                        Identifier = FileMonitoringProvider.Id,
                                        Info = FileMonitoringProvider.Info,
                                        Implementor = typeof(FileMonitoringProvider),
                                        Settings = typeof(FileMonitorProviderPage)
                                    },
                                UdpAppenderListener.ProviderRegistrationInformation,
                                MSBuildAppenderListener.ProviderRegistrationRecord
                            };
        }

        public void Register(IProviderRegistrationRecord record)
        {
            throw new NotImplementedException("Dynamic registration is not yet supported");
        }

        public ILogProvider Create(Guid providerGuid, IProviderSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentException("Settings can not be null", "Settings");
            }

            // Make sure we don't have any instances of that providerGuid.
            if (providerInstances.Any(p => p.Key == settings.Name && p.Value.Information.Identifier == providerGuid))
            {
                throw new ArgumentException(
                    "Already an instance of that ILoggerProvider with that name specified", "settings");
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
                Debug.Assert(record.Implementor != null, "Need to know the implementing type for the provider");

                try
                {
                    var provider = (ILogProvider)Activator.CreateInstance(record.Implementor, settings);
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
                throw new ArgumentException("There is no instance with the identifier " + name, "name");
            }

            return providerInstances.FirstOrDefault(p => p.Key == name).Value;
        }

        public void Remove(string name)
        {
            throw new NotSupportedException("Removal is not yet supported");
        }

        public IEnumerable<Guid> GetRegistered()
        {
            return providers.Select(p => p.Identifier);
        }

        public IProviderInfo GetInformation(Guid providerGuid)
        {
            Debug.Assert(providers.Any(p => p.Identifier == providerGuid), "No such registered Provider");
            if (!providers.Any(p => p.Identifier == providerGuid))
            {
                throw new ArgumentException("Specified guid does not correspond to a registered provider",
                                            "providerGuid");
            }

            return providers.First(p => p.Identifier == providerGuid).Info;
        }

        /// <summary>
        /// Gets the configuration abstraction for the specified Guid.  
        /// Type left to caller to determine (as long as it is satisfied
        /// by the implementer too).
        /// </summary>
        /// <typeparam name="T">Type of configuration</typeparam>
        /// <param name="providerGuid">Identifier of provider</param>
        /// <returns></returns>
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
                    string.Format(
                        "There should only be one registered {0} handler for the provider {1}", typeof(T), providerGuid));
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

        public IEnumerable<ILogProvider> GetInstances()
        {
            return providerInstances.Select(i => i.Value);
        }

        #region Implementation of IEnumerable

        public IEnumerator<Guid> GetEnumerator()
        {
            return providers.Select(p => p.Identifier).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
