#region License
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
#endregion

namespace Sentinel.Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Windows;

    using Sentinel.Interfaces;
    using Sentinel.Support;
    using System.Text;

    public class ServiceLocator
    {
        private static readonly ServiceLocator instance = new ServiceLocator();

        private readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

        private readonly Dictionary<Type, string> fileNames = new Dictionary<Type, string>();

        public string SaveLocation { get; private set; }

        private ServiceLocator()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            SaveLocation = Path.Combine(appData, "Sentinel");

            // Check the folder exists, otherwise create it
            var di = new DirectoryInfo(SaveLocation);
            if (!di.Exists)
            {
                di.Create();
            }
        }

        public static ServiceLocator Instance
        {
            get
            {
                return instance;
            }
        }

        public ReadOnlyCollection<object> RegisteredServices
        {
            get
            {
                return new ReadOnlyCollection<object>(services.Values.ToList());
            }
        }

        public bool ReportErrors { get; set; }

        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "This approach has been chosen as the intended usage style.")]
        public T Get<T>()
        {
            if (services.ContainsKey(typeof(T)))
            {
                return (T)services[typeof(T)];
            }

            if (ReportErrors)
            {
                var errorMessage = string.Format("No registered service supporting {0}", typeof(T));
                MessageBox.Show(errorMessage, "Service location error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return default(T);
        }

        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "The generic style registration is desired, despite this rule.")]
        public bool IsRegistered<T>()
        {
            return services.Keys.Contains(typeof(T));
        }

        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "The generic style registration is desired, despite this rule.")]
        public void Register<T>(object serviceInstance)
        {
            services[typeof(T)] = serviceInstance;
        }

        public void Register(Type keyType, Type instanceType, bool replace)
        {
            if (!services.Keys.Contains(keyType) || replace)
            {
                services[keyType] = Activator.CreateInstance(instanceType);

                var defaultInitialisation = services[keyType] as IDefaultInitialisation;
                if (defaultInitialisation != null)
                {
                    defaultInitialisation.Initialise();
                }
            }
        }

        public void Save()
        {
            foreach (var valuePair in services)
            {
                if (valuePair.Value == null)
                {
                    Trace.TraceError("Unexpected null");
                    continue;
                }

                if (valuePair.Value.HasAttribute<DataContractAttribute>())
                {
                    var saveFileName = fileNames.Get(valuePair.Key) ?? valuePair.Key.Name;
                    var fn = Path.Combine(SaveLocation, saveFileName);
                    JsonHelper.SerializeToFile(valuePair.Value, fn);
                }
            }
        }

        public void RegisterOrLoad<T>(Type interfaceType, string fileName)
        {
            fileNames[interfaceType] = fileName;

            var hasContract = typeof(T).HasAttribute<DataContractAttribute>();
            if (hasContract)
            {
                fileName = Path.ChangeExtension(fileName, ".json");
                fileNames[interfaceType] = fileName;
                var fullName = Path.Combine(SaveLocation, fileName);
                var filterService = JsonHelper.DeserializeFromFile<T>(fullName);
                services[interfaceType] = filterService;
            }

            if (!(services.Keys.Contains(interfaceType) && services[interfaceType] != null))
            {
                try
                {
                    services[interfaceType] = Activator.CreateInstance(typeof(T));
                    var defaultInitialisation = services[interfaceType] as IDefaultInitialisation;
                    if (defaultInitialisation != null)
                    {
                        defaultInitialisation.Initialise();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}