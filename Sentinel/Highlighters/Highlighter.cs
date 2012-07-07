#region License
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
#endregion

namespace Sentinel.Highlighters
{
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using System.Text.RegularExpressions;

    using Sentinel.Highlighters.Interfaces;
    using Sentinel.Interfaces;
    using Sentinel.Support.Mvvm;

    [DataContract]
    public class Highlighter : ViewModelBase, IHighlighter
    {
        #region Backing stores
        private bool enabled = true;

        private LogEntryField field;

        private MatchMode mode;

        private string name;

        private IHighlighterStyle style;

        private string typeMatch;

        private Regex regex;
        #endregion

        public Highlighter()
        {
            Pattern = string.Empty;
            Enabled = false;

            PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName != "Pattern" && e.PropertyName != "Mode")
                    {
                        return;
                    }

                    if (Mode == MatchMode.RegularExpression)
                    {
                        regex = new Regex(Pattern);
                    }
                };
        }

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public bool Enabled
        {
            get
            {
                return enabled;
            }

            set
            {
                if (enabled != value)
                {
                    enabled = value;
                    OnPropertyChanged("Enabled");
                }
            }
        }

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

        public MatchMode Mode
        {
            get
            {
                return mode;
            }

            set
            {
                if (mode != value)
                {
                    mode = value;
                    OnPropertyChanged("Mode");
                }
            }
        }

        public string Pattern
        {
            get
            {
                return typeMatch;
            }

            set
            {
                if (typeMatch != value)
                {
                    typeMatch = value;
                    OnPropertyChanged("Pattern");
                }
            }
        }

        public IHighlighterStyle Style
        {
            get
            {
                return style;
            }

            set
            {
                if (style != value)
                {
                    style = value;
                    OnPropertyChanged("Style");
                }
            }
        }

        public bool IsMatch(ILogEntry logEntry)
        {
            Debug.Assert(logEntry != null, "logEntry can not be null.");
            var target = Field == LogEntryField.System ? logEntry.System : logEntry.Type;

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