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

        private string pattern;

        private Regex regex;

        #endregion

        public Highlighter()
        {
            Pattern = string.Empty;
            Enabled = false;

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "Field" || e.PropertyName == "Mode" || e.PropertyName == "Pattern")
                {
                    if (Mode == MatchMode.RegularExpression && Pattern != null) regex = new Regex(Pattern);
                    OnPropertyChanged("Description");
                }
            };
        }

        public Highlighter(string name, bool enabled, LogEntryField field, MatchMode mode, string pattern, HighlighterStyle style)
        {
            Name = name;
            Enabled = enabled;
            Field = field;
            Mode = mode;
            Pattern = pattern;
            Style = style;
            regex = new Regex(pattern);

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "Field" || e.PropertyName == "Mode" || e.PropertyName == "Pattern")
                {
                    if (Mode == MatchMode.RegularExpression && Pattern != null) regex = new Regex(Pattern);
                    OnPropertyChanged("Description");
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

        public string Description
        {
            get
            {
                string modeDescription = "Exact";
                switch (Mode)
                {
                    case MatchMode.RegularExpression:
                        modeDescription = "RegEx";
                        break;
                    case MatchMode.CaseSensitive:
                        modeDescription = "Case sensitive";
                        break;
                    case MatchMode.CaseInsensitive:
                        modeDescription = "Case insensitive";
                        break;
                }

                return string.Format("{0} match of {1} in the {2} field", modeDescription, Pattern, Field);
            }
        }

        public string Pattern
        {
            get
            {
                return pattern;
            }

            set
            {
                if (pattern != value)
                {
                    pattern = value;
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

            if (string.IsNullOrWhiteSpace(Pattern)) return false;

            string target;

            switch (Field)
            {
                case LogEntryField.None:
                    target = "";
                    break;
                case LogEntryField.Type:
                    target = logEntry.Type;
                    break;
                case LogEntryField.System:
                    target = logEntry.System;
                    break;
                case LogEntryField.Classification:
                    target = "";
                    break;
                case LogEntryField.Thread:
                    target = logEntry.Thread;
                    break;
                //case LogEntryField.Source:
                //    target = logEntry.Source;
                //    break;
                case LogEntryField.Description:
                    target = logEntry.Description;
                    break;
                //case LogEntryField.Host:
                //    target = "";
                //    break;
                default:
                target = "";
                break;
            }

            switch (Mode)
            {
                case MatchMode.Exact:
                    return target.Equals(Pattern);
                case MatchMode.CaseSensitive:
                    return target.Contains(Pattern);
                case MatchMode.CaseInsensitive:
                    return target.ToLower().Contains(Pattern.ToLower());
                case MatchMode.RegularExpression:
                    return regex != null && regex.IsMatch(target);
            }

            return false;
        }
    }
}