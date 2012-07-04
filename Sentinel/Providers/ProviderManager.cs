using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Sentinel.Providers.Interfaces;

namespace Sentinel.Providers
{
    using Sentinel.Interfaces;
    using Sentinel.Interfaces.Providers;

    public class ProviderManager : IProviderManager
    {
        private readonly IList<IProviderRegistrationRecord> providers;

        private readonly List<KeyValuePair<string, ILogProvider>> providerInstances =
            new List<KeyValuePair<string, ILogProvider>>();

        public ProviderManager()
        {
            providers = new List<IProviderRegistrationRecord>
                            {
                                // Note that the Log4net provider only supports UDP and not TCP
                                // so a slightly different configuration page is used.
                                new ProviderRegistrationRecord
                                    {
                                        Identifier = Log4NetProvider.Id,
                                        Info = Log4NetProvider.Info,
                                        Implementor = typeof(Log4NetProvider),
                                        Settings = typeof(UdpNetworkConfigurationPage)
                                    },
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
                                    }
                            };
        }

        public void Register(IProviderRegistrationRecord record)
        {
            throw new NotImplementedException("Dynamic registration is not yet supported");
        }

        public ILogProvider Create(Guid providerGuid, IProviderSettings settings)
        {
            if ( settings == null )
            {
                throw new ArgumentException("Settings can not be null", "Settings");
            }

            // Make sure we don't have any instances of that providerGuid.
            if ( providerInstances.Any(p => p.Key == settings.Name && p.Value.Information.Identifier == providerGuid) )
            {
                throw new ArgumentException("Already an instance of that ILoggerProvider with that name specified",
                                            "settings");
            }

            // Make sure that the type is supported.))
            if (!providers.Any(p => p.Identifier == providerGuid))
            {
                Trace.WriteLine("No provider with the identifier " + providerGuid + " is registered");
                return null;
            }

            // Get an instance.
            ILogProvider provider =
                (ILogProvider) Activator.CreateInstance(
                    providers.First(p => p.Identifier == providerGuid).Implementor,
                    settings);

            providerInstances.Add(new KeyValuePair<string, ILogProvider>(settings.Name, provider));
            return provider;
        }

        public ILogProvider Get(string name)
        {
            Debug.Assert(providerInstances.Any(p => p.Key == name),
                         "There is no instance with the identifier " + name);
            if (!providerInstances.Any(p => p.Key == name))
            {
                throw new ArgumentException("There is no instance with the identifier " + name, "name");
            }

            return providerInstances.Where(p => p.Key == name).FirstOrDefault().Value;
        }

        public void Remove(string name)
        {
            throw new NotSupportedException("Removal is not yet supported");
        }

        public IEnumerable<Guid> GetRegistered()
        {
            return providers.Select(p => p.Identifier);
        }

        public ProviderInfo GetInformation(Guid providerGuid)
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
        /// Gets the configuration abstaction for the specified Guid.  
        /// Type left to caller to determine (as long as it is satified
        /// by the implementer too).
        /// </summary>
        /// <typeparam name="T">Type of configuration</typeparam>
        /// <param name="providerGuid">Identifier of provider</param>
        /// <returns></returns>
        public T GetConfiguration<T>(Guid providerGuid)
        {
            var matchesGuid = providers.Where(p => p.Identifier == providerGuid);

            // Simple checking for duplications.  At the moment, throw an
            // exception if it happens - in the future, it might change to
            // a last-registered wins policy (or maybe first wins!)
            int matchesType = matchesGuid.Count(p => p.Settings.GetInterfaces().Any(i => i.IsAssignableFrom(typeof(T))));

            if ( matchesType > 1 )
            {
                throw new NotSupportedException(
                    string.Format("There should only be one registered {0} handler for the provider {1}", typeof(T),
                                  providerGuid));
            }

            var matches = matchesGuid
                .Where(p => p.Settings.GetInterfaces().Any(i => i.IsAssignableFrom(typeof(T))))
                .Select(p => p.Settings);

            return matches != null && matches.Count() > 0 ? (T) Activator.CreateInstance(matches.Last()) : default(T);
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
