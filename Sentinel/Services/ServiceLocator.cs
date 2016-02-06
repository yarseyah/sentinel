namespace Sentinel.Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Windows;

    using Common.Logging;

    using Sentinel.Interfaces;

    public class ServiceLocator
    {
        private static readonly ILog Log = LogManager.GetLogger<ServiceLocator>();

        private readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

        private ServiceLocator()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            Log.DebugFormat("App Data folder {0}", appData);

            SaveLocation = Path.Combine(appData, "Sentinel");
            Log.DebugFormat("Save location for internal files: {0}", SaveLocation);

            // Check the folder exists, otherwise create it
            var di = new DirectoryInfo(SaveLocation);
            if (!di.Exists)
            {
                Log.TraceFormat("Creating folder {0}", SaveLocation);
                try
                {
                    di.Create();
                }
                catch (Exception e)
                {
                    Log.Error("Unable to create directory", e);
                }
            }
        }

        public static ServiceLocator Instance { get; } = new ServiceLocator();

        public string SaveLocation { get; private set; }

        public ReadOnlyCollection<object> RegisteredServices
        {
            get
            {
                Debug.Assert(services.Values != null, "Values collection should always exist");
                return new ReadOnlyCollection<object>(services.Values.ToList());
            }
        }

        public bool ReportErrors { get; set; }

        [SuppressMessage(
            "Microsoft.Design",
            "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "This approach has been chosen as the intended usage style.")]
        public T Get<T>()
        {
            if (services.ContainsKey(typeof(T)))
            {
                return (T)services[typeof(T)];
            }

            if (ReportErrors)
            {
                var errorMessage = $"No registered service supporting {typeof(T)}";
                Log.Error(errorMessage);
                MessageBox.Show(errorMessage, "Service location error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return default(T);
        }

        [SuppressMessage(
            "Microsoft.Design",
            "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "The generic style registration is desired, despite this rule.")]
        public bool IsRegistered<T>()
        {
            return services.Keys.Contains(typeof(T));
        }

        [SuppressMessage(
            "Microsoft.Design",
            "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "The generic style registration is desired, despite this rule.")]
        public void Register<T>(object serviceInstance)
        {
            services[typeof(T)] = serviceInstance;
        }

        public void Register(Type keyType, Type instanceType, bool replace)
        {
            Log.TraceFormat(
                "Registering Type instance of '{0}' to signature of '{1}'",
                instanceType.Name,
                keyType.Name);

            if (!services.Keys.Contains(keyType) || replace)
            {
                services[keyType] = Activator.CreateInstance(instanceType);

                var defaultInitialisation = services[keyType] as IDefaultInitialisation;
                defaultInitialisation?.Initialise();
            }
        }
    }
}