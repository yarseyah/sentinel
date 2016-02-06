namespace Sentinel.Extractors
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Text.RegularExpressions;

    using Interfaces;
    using Sentinel.Interfaces;

    using WpfExtras;

    [DataContract]
    public class Extractor : ViewModelBase, IExtractor
    {
        /// <summary>
        /// Is the extractor enabled?  If so, it will remove anything matching from the output.
        /// </summary>
        private bool enabled;

        private string name;

        private string pattern;

        private LogEntryField field;

        private MatchMode mode = MatchMode.Exact;

        public Extractor()
        {
            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "Field" || e.PropertyName == "Mode" || e.PropertyName == "Pattern")
                {
                    OnPropertyChanged("Description");
                }
            };
        }

        public Extractor(string name, LogEntryField field, string pattern)
        {
            Name = name;
            Pattern = pattern;
            Field = field;
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

        /// <summary>
        /// Gets or sets a value indicating whether the extractor is enabled.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return enabled;
            }

            set
            {
                if (value != enabled)
                {
                    enabled = value;
                    OnPropertyChanged("Enabled");
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

        public LogEntryField Field
        {
            get
            {
                return field;
            }

            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged("Field");
                }
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
                        modeDescription = "Substring";
                        break;
                }

                return $"{modeDescription} match of {Pattern} in the {Field} field";
            }
        }

        public bool IsMatch(ILogEntry logEntry)
        {
            Debug.Assert(logEntry != null, "LogEntry can not be null.");

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
                    return !target.Equals(Pattern);
                case MatchMode.CaseSensitive:
                    return !target.Contains(Pattern);
                case MatchMode.CaseInsensitive:
                    return !target.ToUpperInvariant().Contains(Pattern.ToUpperInvariant());
                case MatchMode.RegularExpression:
                    var regex = new Regex(Pattern);
                    return !regex.IsMatch(target);
                default:
                    return false;
            }
        }

#if DEBUG
        public override string ToString()
        {
            return Description;
        }

#endif
    }
}
