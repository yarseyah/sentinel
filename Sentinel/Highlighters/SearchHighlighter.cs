#region License
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
#endregion

namespace Sentinel.Highlighters
{
    #region Using directives

    using System.Diagnostics;
    using System.Runtime.Serialization;
    using System.Windows.Media;

    using Sentinel.Highlighters.Interfaces;
    using Sentinel.Interfaces;

    #endregion

    [DataContract]
    public class SearchHighlighter 
        : IDefaultInitialisation, ISearchHighlighter
    {
        private IHighlighter highlighter;

        #region ISearchHighlighter Members

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
        public string Search
        {
            get
            {
                Debug.Assert(highlighter != null, "Must have a highlighter");
                return highlighter.Pattern;
            }

            set
            {
                highlighter.Enabled = !string.IsNullOrEmpty(value);
                highlighter.Pattern = value;
            }
        }

        #endregion

        public void Initialise()
        {
            Highlighter = new Highlighter
            {
                Name = "Search",
                Style = new HighlighterStyle
                {
                    Background = Colors.PaleGreen,
                    Foreground = Colors.Purple
                },
                Field = LogEntryField.System,
                Mode = MatchMode.Substring,
            };

            Search = string.Empty;
        }
    }
}