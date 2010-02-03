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
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Sentinel.Logger;
using Sentinel.Support;

#endregion

namespace Sentinel.Highlighting
{

    #region Using directives

    #endregion

    [Serializable]
    public class Highlighter : ViewModelBase
    {
        private readonly HighlighterData data = new HighlighterData();

        private Regex regex;

        public Highlighter()
        {
            Pattern = string.Empty;
            Enabled = false;

            PropertyChanged += (sender, e) =>
                                   {
                                       if (e.PropertyName == "Pattern" || e.PropertyName == "Mode")
                                       {
                                           if (Mode == MatchMode.RegularExpression)
                                           {
                                               regex = new Regex(Pattern);
                                           }
                                       }
                                   };
        }

        [XmlAttribute]
        public bool Enabled
        {
            get
            {
                return data.Enabled;
            }

            set
            {
                if (data.Enabled != value)
                {
                    data.Enabled = value;
                    OnPropertyChanged("Enabled");
                }
            }
        }

        public LogEntryField Field
        {
            get
            {
                return data.Field;
            }

            set
            {
                data.Field = value;
                OnPropertyChanged("Field");
            }
        }

        public string HighlighterType
        {
            get
            {
                return "Basic Highlighter";
            }
        }

        public MatchMode Mode
        {
            get
            {
                return data.Mode;
            }

            set
            {
                if (data.Mode != value)
                {
                    data.Mode = value;
                    OnPropertyChanged("Mode");
                }
            }
        }

        [XmlAttribute]
        public string Name
        {
            get
            {
                return data.Name;
            }

            set
            {
                if (data.Name != value)
                {
                    data.Name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public string Pattern
        {
            get
            {
                return data.TypeMatch;
            }

            set
            {
                if (data.TypeMatch != value)
                {
                    data.TypeMatch = value;
                    OnPropertyChanged("Pattern");
                }
            }
        }

        public HighlighterStyle Style
        {
            get
            {
                return data.Style;
            }

            set
            {
                if (data.Style != value)
                {
                    data.Style = value;
                    OnPropertyChanged("Style");
                }
            }
        }

        public bool IsMatch(ILogEntry logEntry)
        {
            Debug.Assert(logEntry != null, "logEntry can not be null.");
            string target = Field == LogEntryField.System ? logEntry.System : logEntry.Type;

            switch (Mode)
            {
                case MatchMode.Exact:
                    return target.Equals(Pattern);
                case MatchMode.Substring:
                    return target.Contains(Pattern);
                case MatchMode.RegularExpression:
                    return regex != null && regex.IsMatch(target);
            }

            return false;
        }
    }
}