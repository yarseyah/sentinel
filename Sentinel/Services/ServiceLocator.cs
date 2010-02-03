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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;

#endregion

namespace Sentinel.Services
{

    #region Using directives

    #endregion

    public class ServiceLocator
    {
        private static readonly ServiceLocator instance = new ServiceLocator();

        private readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

        private ServiceLocator()
        {
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

        [SuppressMessage(
            "Microsoft.Design",
            "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "This approach has been chosen as the intended usage style.")]
        public T Get<T>()
        {
            object service = null;
            if (services.ContainsKey(typeof(T)))
            {
                service = services[typeof(T)];
            }

            if (service == null && ReportErrors)
            {
                string errorMessage = String.Format("No registered service supporting {0}", typeof(T));
                MessageBox.Show(
                    errorMessage,
                    "Service location error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            return (T) service;
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
                                string type = reader.ReadContentAsString();

                                reader.ReadStartElement("Service");

                                XmlSerializer serializer = new XmlSerializer(Type.GetType(type));
                                object o = serializer.Deserialize(reader);
                                services.Add(Type.GetType(key), o);

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
            }
        }

        public void Save(string fileName)
        {
            XmlWriterSettings writerSettings = new XmlWriterSettings
                                                   {
                                                       OmitXmlDeclaration = true,
                                                       Indent = true,
                                                       IndentChars = "    "
                                                   };

            using (XmlWriter xmlWriter = XmlWriter.Create(fileName, writerSettings))
            {
                xmlWriter.WriteStartElement("Services");

                int count = services.Count(s => IsSerializable(s.Value));
                xmlWriter.WriteAttributeString("count", count.ToString());

                foreach (KeyValuePair<Type, object> valuePair in services)
                {
                    if (IsSerializable(valuePair.Value))
                    {
                        xmlWriter.WriteStartElement("Service");

                        xmlWriter.WriteAttributeString("key", valuePair.Key.ToString());
                        xmlWriter.WriteAttributeString("instanceType", valuePair.Value.GetType().ToString());

                        XmlSerializer serializer = new XmlSerializer(valuePair.Value.GetType());
                        serializer.Serialize(xmlWriter, valuePair.Value);

                        xmlWriter.WriteEndElement();
                    }
                }

                xmlWriter.WriteEndElement();
            }
        }

        private static bool IsSerializable(object obj)
        {
            return obj is ISerializable ||
                   Attribute.IsDefined(obj.GetType(), typeof(SerializableAttribute));
        }
    }
}