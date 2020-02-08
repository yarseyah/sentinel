namespace Sentinel.Extractors
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Text.RegularExpressions;

    using Sentinel.Extractors.Interfaces;
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

        private LogEntryFields field;

        private MatchMode mode = MatchMode.Exact;

        public Extractor()
        {
            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "Field" || e.PropertyName == "Mode" || e.PropertyName == "Pattern")
                {
                    OnPropertyChanged(nameof(Description));
                }
            };
        }

        public Extractor(string name, LogEntryFields field, string pattern)
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
                    OnPropertyChanged(nameof(Name));
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
                    OnPropertyChanged(nameof(Enabled));
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
                    OnPropertyChanged(nameof(Pattern));
                }
            }
        }

        public LogEntryFields Field
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
                    OnPropertyChanged(nameof(Field));
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
                case LogEntryFields.None:
                    target = string.Empty;
                    break;
                case LogEntryFields.Type:
                    target = logEntry.Type;
                    break;
                case LogEntryFields.System:
                    target = logEntry.System;
                    break;
                case LogEntryFields.Classification:
                    target = string.Empty;
                    break;
                case LogEntryFields.Thread:
                    target = logEntry.Thread;
                    break;
                case LogEntryFields.Source:
                    target = logEntry.Source;
                    break;
                case LogEntryFields.Description:
                    target = logEntry.Description;
                    break;
                case LogEntryFields.Host:
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
