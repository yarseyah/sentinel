using Sentinel.Extractors.Interfaces;
using Sentinel.Interfaces;
using Sentinel.Support.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sentinel.Extractors
{
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

        #region IExtractor Members

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

                return string.Format("{0} match of {1} in the {2} field", modeDescription, Pattern, Field);
            }
        }

        public bool IsMatch(ILogEntry logEntry)
        {
            Debug.Assert(logEntry != null, "LogEntry can not be null.");

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
                    return !target.Equals(Pattern);
                case MatchMode.CaseSensitive:
                    return !target.Contains(Pattern);
                case MatchMode.CaseInsensitive:
                    return !target.ToLower().Contains(Pattern.ToLower());
                case MatchMode.RegularExpression:
                    var regex = new Regex(Pattern);
                    return regex != null && !regex.IsMatch(target);
                default:
                    return false;
            }
        }
        #endregion

#if DEBUG
        public override string ToString()
        {
            return Description;
        }
#endif

    }
}
