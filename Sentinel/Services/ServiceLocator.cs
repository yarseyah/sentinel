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
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.ComponentModel.Composition.Diagnostics;
using Microsoft.ComponentModel.Composition.DynamicInstantiation;
using Sentinel.Views;
using Sentinel.Views.Interfaces;

#endregion

namespace Sentinel.Services
{
    [Export]
    public class ServiceLocator
    {
        private static readonly ServiceLocator instance = new ServiceLocator();

        private readonly CompositionContainer compositionContainer;

        private readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

        private ServiceLocator()
        {
            var cat = new AssemblyCatalog(Assembly.GetAssembly(GetType()));
            var dynamicProvider = new DynamicInstantiationExportProvider();
            compositionContainer = new CompositionContainer(cat, dynamicProvider);
            dynamicProvider.SourceProvider = compositionContainer;

            compositionContainer.ComposeParts(this);
            var ci = new CompositionInfo(cat, compositionContainer);

            // Manually register some 'services' that are not discovered
            Register<IViewManager>(new ViewManager());

            StringBuilder sb = new StringBuilder();
            using ( StringWriter sw = new StringWriter(sb) )
            {
                CompositionInfoTextFormatter.Write(ci, sw);
            }
            Trace.WriteLine(sb.ToString());
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
            if (services.ContainsKey(typeof(T)))
            {
                return (T) services[typeof(T)];
            }

            try
            {
                Lazy<T> mefInstance = compositionContainer.GetExport<T>();
                if (mefInstance != null)
                {
                    return mefInstance.Value;
                }
            }
            catch (CompositionException mefException)
            {
                Trace.WriteLine(mefException.Message);
            }
            catch (ImportCardinalityMismatchException mefException)
            {
                Trace.WriteLine(mefException.Message);
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

        private Type LookupRuntimeType(string typeName)
        {
            Type t = Type.GetType(typeName);
            if ( t != null )
            {
                return t;
            }

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                t = assembly.GetType(typeName);
                if ( t != null )
                {
                    return t;
                }
            }

            throw new NotSupportedException("The type can not be found in any loaded assemblies: " + typeName);
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