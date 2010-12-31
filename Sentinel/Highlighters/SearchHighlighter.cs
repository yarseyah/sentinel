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
using ProtoBuf;
using Sentinel.Highlighters.Interfaces;
using Sentinel.Interfaces;

#endregion

namespace Sentinel.Highlighters
{
    [ProtoContract]
    public class SearchHighlighter 
        : IDefaultInitialisation
        , ISearchHighlighter
    {
        private Highlighter highlighter;

        #region ISearchHighlighter Members

        [ProtoMember(1)]
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


        [ProtoMember(2)]
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

        [ProtoMember(3)]
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
            Search = String.Empty;
        }
    }
}