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
    public class SearchHighlighter : IDefaultInitialisation, ISearchHighlighter
    {
        [DataMember]
        public IHighlighter Highlighter { get; set; }

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
                return Highlighter.Field;
            }

            set
            {
                Highlighter.Field = value;
            }
        }

        [DataMember]
        public bool Enabled
        {
            get
            {
                return Highlighter.Enabled;
            }

            set
            {
                Highlighter.Enabled = value;
            }
        }

        [DataMember]
        public MatchMode Mode
        {
            get
            {
                return Highlighter.Mode;
            }

            set
            {
                Highlighter.Mode = value;
            }
        }

        [DataMember]
        public string Search
        {
            get
            {
                Debug.Assert(Highlighter != null, "Must have a highlighter");
                return Highlighter.Pattern;
            }

            set
            {
                Highlighter.Pattern = value;
            }
        }

        public void Initialise()
        {
            Highlighter = new Highlighter
                              {
                                  Name = "Search",
                                  Style =
                                      new HighlighterStyle
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