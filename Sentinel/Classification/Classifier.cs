namespace Sentinel.Classification
{
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using System.Text.RegularExpressions;

    using Interfaces;
    using Sentinel.Interfaces;

    using WpfExtras;

    [DataContract]
    public class Classifier : ViewModelBase, IClassifier
    {
        private bool enabled = true;

        private LogEntryField field;

        private MatchMode mode;

        private string name;

        private string type;

        private string pattern;

        private Regex regex;

        public Classifier()
        {
            PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == "Field" || e.PropertyName == "Mode" || e.PropertyName == "Pattern")
                    {
                        if (Mode == MatchMode.RegularExpression && Pattern != null)
                        {
                            regex = new Regex(Pattern);
                        }

                        OnPropertyChanged("Description");
                    }
                };
        }

        public Classifier(string name, bool enabled, LogEntryField field, MatchMode mode, string pattern, string type)
        {
            Name = name;
            Enabled = enabled;
            Field = field;
            Mode = mode;
            Pattern = pattern;
            Type = type;
            regex = new Regex(pattern);

            PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == "Field" || e.PropertyName == "Mode" || e.PropertyName == "Pattern")
                    {
                        if (Mode == MatchMode.RegularExpression && Pattern != null)
                        {
                            regex = new Regex(Pattern);
                        }

                        OnPropertyChanged("Description");
                    }
                };
        }

        public string Description
        {
            get
            {
                var modeDescription = "Exact";

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
                    OnPropertyChanged("Mode");
                }
            }
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

        public string Type
        {
            get
            {
                return type;
            }

            set
            {
                if (type != value)
                {
                    type = value;
                    OnPropertyChanged("Type");
                }
            }
        }

        public ILogEntry Classify(ILogEntry logEntry)
        {
            Debug.Assert(logEntry != null, "logEntry can not be null.");

            if (IsMatch(logEntry))
            {
                logEntry.MetaData["Classification"] = Type;
                logEntry.Type = Type;
            }

            return logEntry;
        }

        public bool IsMatch(ILogEntry logEntry)
        {
            Debug.Assert(logEntry != null, "logEntry can not be null.");

            if (string.IsNullOrWhiteSpace(Pattern))
            {
                return false;
            }

            string target;

            switch (Field)
            {
                case LogEntryField.None:
                    target = string.Empty;
                    break;
                case LogEntryField.Type:
                    target = logEntry.Type;
                    break;
                case LogEntryField.System:
                    target = logEntry.System;
                    break;
                case LogEntryField.Classification:
                    target = string.Empty;
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
                case LogEntryField.Host:
                    target = string.Empty;
                    break;
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