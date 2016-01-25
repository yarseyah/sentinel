namespace Sentinel.Highlighters
{
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using System.Text.RegularExpressions;

    using Interfaces;
    using Sentinel.Interfaces;

    using WpfExtras;

    [DataContract]
    public class Highlighter : ViewModelBase, IHighlighter
    {
        private bool enabled = true;

        private LogEntryField field;

        private MatchMode mode;

        private string name;

        private IHighlighterStyle style;

        private string pattern;

        private Regex regex;

        public Highlighter()
        {
            Pattern = string.Empty;
            Enabled = false;

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(Field) || e.PropertyName == nameof(Mode) || e.PropertyName == nameof(Pattern))
                {
                    if (Mode == MatchMode.RegularExpression && Pattern != null)
                    {
                        regex = new Regex(Pattern);
                    }

                    OnPropertyChanged(nameof(Description));
                }
            };
        }

        protected Highlighter(string name, bool enabled, LogEntryField field, MatchMode mode, string pattern, IHighlighterStyle style)
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
                if (e.PropertyName == nameof(Field) || e.PropertyName == nameof(Mode) ||
                    e.PropertyName == nameof(Pattern))
                {
                    if (Mode == MatchMode.RegularExpression && Pattern != null)
                    {
                        regex = new Regex(Pattern);
                    }

                    OnPropertyChanged(nameof(Description));
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
                    OnPropertyChanged(nameof(Name));
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
                    OnPropertyChanged(nameof(Enabled));
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
                OnPropertyChanged(nameof(Field));
            }
        }

        public string HighlighterType => "Basic Highlighter";

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
                    OnPropertyChanged(nameof(Mode));
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

                return $"{modeDescription} match of {Pattern} in the {Field} field";
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

            if (logEntry == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(Pattern))
            {
                return false;
            }

            string target;

            switch (Field)
            {
                case LogEntryField.Type:
                    target = logEntry.Type;
                    break;
                case LogEntryField.System:
                    target = logEntry.System;
                    break;
                case LogEntryField.Thread:
                    target = logEntry.Thread;
                    break;
                case LogEntryField.Source:
                    target = logEntry.Source;
                    break;
                case LogEntryField.Description:
                    target = logEntry.Description;
                    break;
                ////case LogEntryField.Classification:
                ////case LogEntryField.None:
                ////case LogEntryField.Host:
                default:
                    target = string.Empty;
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