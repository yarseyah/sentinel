#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

using System.Windows.Media;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using ProtoBuf;
using Sentinel.Interfaces;
using Sentinel.Support.Mvvm;

#endregion

namespace Sentinel.Highlighters
{
    [ProtoContract]
    public class HighlighterStyle 
        : ViewModelBase
        , IHighlighterStyle
    {
        private Color? background;

        private Color? foreground;

        [ProtoMember(1)]
        public string BackgroundAsString
        {
            get
            {
                return Background != null ? Background.Value.ToString() : string.Empty;
            }
            set
            {
                Background = !string.IsNullOrWhiteSpace(value) 
                    ? (Color?) ColorConverter.ConvertFromString(value) 
                    : null;
            }
        }

        [ProtoMember(2)]
        public string ForegroundAsString
        {
            get
            {
                return Foreground != null ? Foreground.Value.ToString() : string.Empty;
            }
            set
            {
                Foreground = !string.IsNullOrWhiteSpace(value)
                    ? (Color?)ColorConverter.ConvertFromString(value)
                    : null;
            }
        }


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
    }
}