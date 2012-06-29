#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

namespace Sentinel.Highlighters
{
    using System.Runtime.Serialization;
    using System.Windows.Media;

    using Sentinel.Interfaces;
    using Sentinel.Support.Mvvm;

    [DataContract]
    public class HighlighterStyle 
        : ViewModelBase
        , IHighlighterStyle
    {
        private Color? background;

        private Color? foreground;

        [DataMember]
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

        [DataMember]
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

        // TODO: can these be serialized directly with a converter?
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

        // TODO: can these be serialized directly with a converter?
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