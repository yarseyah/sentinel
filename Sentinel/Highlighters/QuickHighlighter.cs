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
using System.Runtime.Remoting.Contexts;
using System.Runtime.Serialization;
using System.Windows.Media;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Sentinel.Highlighters.Interfaces;
using Sentinel.Interfaces;

#endregion

namespace Sentinel.Highlighters
{
    #region Using directives

    #endregion

    [Serializable]
    public class QuickHighlighter : IQuickHighlighter, IXmlSerializable
    {
        private Highlighter highlighter;

        public QuickHighlighter()
        {
            highlighter = new Highlighter
                                {
                                    Name = "Quick Highlight",
                                    Style = new HighlighterStyle
                                                {
                                                    Background = Colors.PaleGreen,
                                                    Foreground = Colors.Purple
                                                },
                                    Field = LogEntryField.System,
                                    Mode = MatchMode.Substring,
                                };
            Search = String.Empty;
        }

        #region IQuickHighlighter Members

        public LogEntryField Field
        {
            get
            {
                return highlighter.Field;
            }

            set
            {
                highlighter.Field = value;
            }
        }

        public Highlighter Highlighter
        {
            get
            {
                return highlighter;
            }

            set
            {
                highlighter = value;
            }
        }

        public string Search
        {
            get
            {
                return highlighter.Pattern;
            }

            set
            {
                highlighter.Enabled = !string.IsNullOrEmpty(value);
                highlighter.Pattern = value;
            }
        }

        #endregion

        #region Implementation of IXmlSerializable

        /// <summary>
        /// This method is reserved and should not be used. When implementing the 
        /// IXmlSerializable interface, you should return null (Nothing in Visual Basic) 
        /// from this method, and instead, if specifying a custom schema is required, 
        /// apply the <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute"/> 
        /// to the class.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Xml.Schema.XmlSchema"/> that describes the XML representation
        /// of the object that is produced by the 
        /// <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)"/> 
        /// method and consumed by the 
        /// <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)"/> 
        /// method.
        /// </returns>
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Generates an object from its XML representation.
        /// </summary>
        /// <param name="reader">The <see cref="T:System.Xml.XmlReader"/> stream from which the object is deserialized. 
        ///                 </param>
        public void ReadXml(XmlReader reader)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(int), new XmlRootAttribute("QuickHighlighter"));
            object o = xmlSerializer.Deserialize(reader);
            Field = (LogEntryField) o;

            xmlSerializer = new XmlSerializer(typeof(string));
            Search = (string)xmlSerializer.Deserialize(reader);

            xmlSerializer = new XmlSerializer(typeof(Highlighter));
            highlighter = (Highlighter) xmlSerializer.Deserialize(reader);
        }

        /// <summary>
        /// Converts an object into its XML representation.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Xml.XmlWriter"/> stream to which the object is serialized. 
        ///                 </param>
        public void WriteXml(XmlWriter writer)
        {
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(int));
            xmlSerializer.Serialize(writer, Field, ns);

            xmlSerializer = new XmlSerializer(typeof(string));
            xmlSerializer.Serialize(writer, Search, ns);

            xmlSerializer = new XmlSerializer(typeof(Highlighter));
            xmlSerializer.Serialize(writer, highlighter, ns);
        }

        #endregion
    }
}