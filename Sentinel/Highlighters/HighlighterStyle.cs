#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

using System.ComponentModel.Composition;
using System.Windows.Media;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Sentinel.Interfaces;
using Sentinel.Support.Mvvm;

#endregion

namespace Sentinel.Highlighters
{
    [Export(typeof(IHighlighterStyle))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class HighlighterStyle 
        : ViewModelBase
        , IXmlSerializable
        , IHighlighterStyle
    {
        private Color? background;

        private Color? foreground;

        [XmlAttribute]
        public Color? Background
        {
            get
            {
                return background;
            }

            set
            {
                if (value != background)
                {
                    background = value;
                    OnPropertyChanged("Background");
                }
            }
        }

        [XmlAttribute]
        public Color? Foreground
        {
            get
            {
                return foreground;
            }

            set
            {
                if (value != foreground)
                {
                    foreground = value;
                    OnPropertyChanged("Foreground");
                }
            }
        }

        #region IXmlSerializable Members

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            if (reader.AttributeCount == 2)
            {
                string fg = reader.GetAttribute("Fg");
                string bg = reader.GetAttribute("Bg");

                if (fg == "(null)")
                {
                    Foreground = null;
                }
                else
                {
                    Foreground = (Color?)ColorConverter.ConvertFromString(fg);
                }

                if (bg == "(null)")
                {
                    Background = null;
                }
                else
                {
                    Background = (Color?)ColorConverter.ConvertFromString(bg);
                }
            }
            else
            {
                throw new XmlSchemaException("HighlighterStyle should have exactly two attributes");
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Fg", Foreground == null ? "(null)" : Foreground.Value.ToString());
            writer.WriteAttributeString("Bg", Background == null ? "(null)" : Background.Value.ToString());
        }

        #endregion
    }
}