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
using System.Windows.Media;
using System.Xml.Serialization;
using Sentinel.Logger;
using Sentinel.Support;

#endregion

namespace Sentinel.Highlighting
{

    #region Using directives

    #endregion

    [Serializable]
    public class QuickHighlighter : IQuickHighlighter
    {
        private Highlighter highlighter;

        public QuickHighlighter()
        {
            Highlighter h = new Highlighter
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
            highlighter = h;

            Search = "dataLink";
        }

        #region IQuickHighlighter Members

        [XmlIgnore]
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

        [XmlIgnore]
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
    }
}