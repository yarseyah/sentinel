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
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Sentinel.Classification.Interfaces;
using Sentinel.Interfaces;

#endregion

namespace Sentinel.Classification
{
    /// <summary>
    /// Log message classifier that works by processing the description field.
    /// </summary>
    public class DescriptionClassifier : VisualClassifier
    {
        private string regexString;

        private Regex regularExpression;

        public Dictionary<string, object> Substutions { get; set; }

        /// <summary>
        /// Initializes a new instance of the DescriptionClassifier class.
        /// </summary>
        public DescriptionClassifier()
            : base("Description Classifier")
        {
        }

        /// <summary>
        /// Gets or sets the regular expression string to use for the processing of
        /// description fields.
        /// </summary>
        /// <remarks>Exception handling for malformed regex needs improving.</remarks>
        public string RegexString
        {
            get
            {
                return regexString;
            }

            set
            {
                if (value != regexString)
                {
                    regexString = value;

                    // TODO: Improve exception handling.
                    regularExpression = (value != string.Empty)
                                            ? new Regex(value, RegexOptions.Compiled)
                                            : null;
                }
            }
        }

        #region IDescriptionClassifier Members

        public override bool IsMatch(object parameter)
        {
            return regularExpression != null && regularExpression.Match((string) parameter).Success;
        }

        public override ILogEntry Classify(ILogEntry entry)
        {
            Debug.Assert(Enabled, "Should not be attempting to classify using non-enabled classifiers.");

            if (regularExpression == null
                || !Enabled)
            {
                return entry;
            }

            Match m = regularExpression.Match(entry.Description);
            if (m.Success)
            {
                entry.System = regexString.Contains("(?<system>")
                                   ? m.Groups["system"].Value
                                   : Substutions != null && Substutions.ContainsKey("system")
                                         ? Substutions["system"].ToString()
                                         : entry.System;
                entry.Description = regexString.Contains("(?<description>")
                                        ? m.Groups["description"].Value
                                        : Substutions != null && Substutions.ContainsKey("description")
                                              ? Substutions["description"].ToString()
                                              : entry.Description;
                entry.Type = regexString.Contains("(?<type>")
                                 ? m.Groups["type"].Value
                                 : Substutions != null && Substutions.ContainsKey("type")
                                       ? Substutions["type"].ToString()
                                       : entry.Type;

                if ( entry.MetaData == null )
                {
                    entry.MetaData = new Dictionary<string, object>();
                }
                entry.MetaData["Classification"] = Type;
            }

            return entry;
        }

        #endregion
    }
}