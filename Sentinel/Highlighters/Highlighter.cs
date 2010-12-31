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
using ProtoBuf;
using Sentinel.Highlighters.Interfaces;
using Sentinel.Interfaces;
using Sentinel.Support.Mvvm;

#endregion

namespace Sentinel.Highlighters
{
    [ProtoContract(Name = "Highlighter")]
    public class Highlighter : ViewModelBase
    {
        #region Backing stores
        private bool enabled = true;

        private LogEntryField field;

        private MatchMode mode;

        private string name;

        private HighlighterStyle style;

        private string typeMatch;

        private Regex regex;
        #endregion

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

        [ProtoMember(1)]
        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                if (name == value) return;
                name = value;
                OnPropertyChanged("Name");
            }
        }

        [ProtoMember(2)]
        public bool Enabled
        {
            get
            {
                return enabled;
            }

            set
            {
                if (enabled == value) return;
                enabled = value;
                OnPropertyChanged("Enabled");
            }
        }

        [ProtoMember(3)]
        public LogEntryField Field
        {
            get
            {
                return field;
            }

            set
            {
                field = value;
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

        [ProtoMember(4)]
        public MatchMode Mode
        {
            get
            {
                return mode;
            }

            set
            {
                if (mode == value) return;
                mode = value;
                OnPropertyChanged("Mode");
            }
        }

        [ProtoMember(5)]
        public string Pattern
        {
            get
            {
                return typeMatch;
            }

            set
            {
                if (typeMatch == value) return;
                typeMatch = value;
                OnPropertyChanged("Pattern");
            }
        }

        [ProtoMember(6)]
        public HighlighterStyle Style
        {
            get
            {
                return style;
            }

            set
            {
                if (style == value) return;
                style = value;
                OnPropertyChanged("Style");
            }
        }

        public bool IsMatch(LogEntry logEntry)
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

        #region Protobuf attributed methods (for breakpointing)
        [ProtoAfterDeserialization]
        public void PostLoad()
        {
        }

        [ProtoBeforeSerialization]
        public void PreSave()
        {
        }
        #endregion
    }
}