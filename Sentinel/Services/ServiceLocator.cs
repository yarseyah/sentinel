#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Windows;
using ProtoBuf;
using Sentinel.Interfaces;

#endregion

namespace Sentinel.Services
{
    using System.Runtime.Serialization;

    using Sentinel.Support;

    public class ServiceLocator
    {
        private static readonly ServiceLocator instance = new ServiceLocator();

        private readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

        private readonly Dictionary<Type, string> fileNames = new Dictionary<Type, string>(); 

        public string SaveLocation { get; private set; }

        private ServiceLocator()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            SaveLocation = Path.Combine(appData, "Sentinel");

            // Check the folder exists, otherwise create it
            DirectoryInfo di = new DirectoryInfo(SaveLocation);
            if (!di.Exists)
            {
                di.Create();
            }
        }

        public static ServiceLocator Instance { get { return instance; } }

        public ReadOnlyCollection<object> RegisteredServices
        {
            get
            {
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
                return (T) services[typeof(T)];
            }

            if (ReportErrors)
            {
                string errorMessage = String.Format("No registered service supporting {0}", typeof(T));
                MessageBox.Show(
                    errorMessage,
                    "Service location error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
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
            if (!services.Keys.Contains(keyType) || replace)
            {
                services[keyType] = Activator.CreateInstance(instanceType);
                if (services[keyType] is IDefaultInitialisation)
                {
                    ((IDefaultInitialisation) services[keyType]).Initialise();
                }
            }
        }

        public void Save()
        {
            foreach (KeyValuePair<Type, object> valuePair in services)
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
                    continue;
                }

                Trace.WriteLine(string.Format("{0} - {1}", valuePair.Key, valuePair.Value));
                if (!IsProtobufSerializable(valuePair.Value))
                {
                    Trace.WriteLine("Doesn't support saving.");
                    continue;
                }

                Trace.WriteLine("attempting to save.");

                try
                {
                    MemoryStream ms = new MemoryStream();
                    Serializer.Serialize(ms, valuePair.Value);

                    var typeName = valuePair.Key.FullName ?? "Unknown";
                    var saveFileName = fileNames.Get(valuePair.Key) ?? typeName;
                    var fullName = Path.Combine(SaveLocation, saveFileName);

                    ms.Position = 0;
                    var fi = new FileInfo(fullName);
                    using (var fs = fi.Open(FileMode.Create, FileAccess.Write))
                    {
                        ms.CopyTo(fs);
                        Trace.WriteLine(string.Format("Wrote {0} data to {1}", typeName, fullName));
                    }
                }
                catch (Exception e)
                {
                    Trace.WriteLine("Exception caught in proto-saving:");
                    Trace.WriteLine(e.Message);
                }
            }
        }

        private static bool IsProtobufSerializable(object value)
        {
            if (value == null) throw new ArgumentNullException("value");
            return Attribute.IsDefined(value.GetType(), typeof(ProtoContractAttribute));
        }

        public void RegisterOrLoad<T>(Type interfaceType, string fileName)
        {
            fileNames[interfaceType] = fileName;

            var fullName = Path.Combine(SaveLocation, fileName);

            var hasContract = typeof(T).HasAttribute<DataContractAttribute>();
            if (hasContract)
            {
                // TODO: this is duplicating above, but adding the .json, remove when all are JSON.
                fileName = Path.ChangeExtension(fileName, ".json");
                fileNames[interfaceType] = fileName;
                fullName = Path.Combine(SaveLocation, fileName);

                var filterService = JsonHelper.DeserializeFromFile<T>(fullName);
                services[interfaceType] = filterService;
            }
            else
            {
                var fi = new FileInfo(fullName);
                if (fi.Exists)
                {
                    {
                        using (var fs = fi.OpenRead())
                        {
                            try
                            {
                                services[interfaceType] = Serializer.Deserialize<T>(fs);
                                Debug.WriteLine(
                                    "Loaded {0} with settings in {1}",
                                    services[interfaceType].GetType().FullName,
                                    fileName);
                            }
                            catch (Exception e)
                            {
                                Trace.WriteLine(
                                    string.Format("Exception when trying to de-serialize from {0}", fileName));
                                Trace.WriteLine(e.Message);
                            }
                        }
                    }
                }
            }

            var isRegistered = services.Keys.Contains(interfaceType) &&
                               services[interfaceType] != null;

            if (isRegistered) return;

            // Nothing serializeable, try to construct and then see whether it supports initialization.
            try
            {
                services[interfaceType] = Activator.CreateInstance(typeof (T));
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