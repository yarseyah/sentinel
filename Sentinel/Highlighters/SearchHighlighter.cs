namespace Sentinel.Highlighters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Windows.Media;

    using Sentinel.Highlighters.Interfaces;
    using Sentinel.Interfaces;

    [DataContract]
    public class SearchHighlighter 
        : IDefaultInitialisation, ISearchHighlighter
    {
        private IHighlighter highlighter;

        [DataMember]
        public IHighlighter Highlighter
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

        public IEnumerable<LogEntryField> Fields
        {
            get
            {
                var entries = Enum.GetValues(typeof(LogEntryField)).Cast<LogEntryField>();
                return entries;
            }
        }

        [DataMember]
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

        [DataMember]
        public bool Enabled
        {
            get
            {
                return highlighter.Enabled;
            }

            set
            {
                highlighter.Enabled = value;
            }
        }

        [DataMember]
        public MatchMode Mode
        {
            get
            {
                return highlighter.Mode;
            }

            set
            {
                highlighter.Mode = value;
            }
        }

        [DataMember]
        public string Search
        {
            get
            {
                Debug.Assert(highlighter != null, "Must have a highlighter");
                return highlighter.Pattern;
            }

            set
            {                
                highlighter.Pattern = value;
            }
        }

        public void Initialise()
        {
            Highlighter = new Highlighter
            {
                Name = "Search",
                Style = new HighlighterStyle
                {
                    Background = Colors.Lime,
                    Foreground = Colors.Fuchsia
                },
                Field = LogEntryField.System,
                Mode = MatchMode.CaseSensitive,
            };

            Search = string.Empty;
        }
    }
}