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
using System.Xml;
using System.Xml.Serialization;
using ProtoBuf;
using Sentinel.Interfaces;

#endregion

namespace Sentinel.Services
{
    public class ServiceLocator
    {
        private static readonly ServiceLocator instance = new ServiceLocator();

        private readonly string directoryForSaving;

        private readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

        private ServiceLocator()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            directoryForSaving = Path.Combine(appData, "Sentinel");
            // Check the folder exists, otherwise create it
            DirectoryInfo di = new DirectoryInfo(directoryForSaving);
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

        public void Load(string fileName)
        {
            FileInfo fi = new FileInfo(fileName);

            if (fi.Exists)
            {
                XmlReaderSettings readerSettings = new XmlReaderSettings {IgnoreWhitespace = true};

                using (XmlReader reader = XmlReader.Create(fi.FullName, readerSettings))
                {
                    reader.MoveToContent();
                    if (reader.Name == "Services" || reader.AttributeCount == 1)
                    {
                        reader.MoveToAttribute("count");
                        int count = reader.ReadContentAsInt();
                        reader.ReadStartElement();

                        for (int i = 0; i < count; i++)
                        {
                            if (reader.Name == "Service" && reader.AttributeCount == 2)
                            {
                                reader.MoveToAttribute("key");
                                string key = reader.ReadContentAsString();
                                reader.MoveToAttribute("instanceType");
                                string typeAsString = reader.ReadContentAsString();
                                
                                Type type = LookupRuntimeType(typeAsString);
                                reader.ReadStartElement("Service");

                                XmlSerializer serializer = new XmlSerializer(type);
                                object o = serializer.Deserialize(reader);
                                services.Add(LookupRuntimeType(key), o);

                                reader.ReadEndElement();
                            }
                            else
                            {
                                throw new NotSupportedException("Should be a Service node with exactly two attributes");
                            }
                        }

                        reader.ReadEndElement();
                    }
                    else
                    {
                        throw new NotSupportedException("There should be exactly one attribute for Services");
                    }
                }
            }
        }

        private static Type LookupRuntimeType(string typeName)
        {
            if (String.IsNullOrWhiteSpace(typeName)) throw new ArgumentNullException("typeName");

            Type t = Type.GetType(typeName);
            return t;
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

                    string typeName = valuePair.Key.FullName ?? "Unknown";
                    string saveFileName = GetShortName(typeName) ?? typeName;
                    string fullName = Path.Combine(directoryForSaving, saveFileName);

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

        private static string GetShortName(string typeName)
        {
            if (typeName.Contains("Filter")) return "Filters";
            if (typeName.Contains("HighlightingService")) return "Highlighters";
            return null;
        }

        private static bool IsProtobufSerializable(object value)
        {
            if (value == null) throw new ArgumentNullException("value");
            return Attribute.IsDefined(value.GetType(), typeof(ProtoContractAttribute));
        }

        public void RegisterOrLoad<T>(Type interfaceType, string fileName)
        {
            string fullName = Path.Combine(directoryForSaving, fileName);

            FileInfo fi = new FileInfo(fullName);
            bool found = false;
            if (fi.Exists)
            {
                using (var fs = fi.OpenRead())
                {
                    try
                    {
                        services[interfaceType] = Serializer.Deserialize<T>(fs);
                        Debug.WriteLine("Loaded {0} with settings in {1}",
                                        services[interfaceType].GetType().FullName,
                                        fileName);
                        found = true;
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine(string.Format("Exception when trying to deserialize from {0}", fileName));
                        Trace.WriteLine(e.Message);
                    }
                }
            }

            if (found) return;
            if (services.Keys.Contains(interfaceType)) return;

            services[interfaceType] = Activator.CreateInstance(typeof(T));
            if (services[interfaceType] is IDefaultInitialisation)
            {
                ((IDefaultInitialisation)services[interfaceType]).Initialise();
            }
        }
    }
}