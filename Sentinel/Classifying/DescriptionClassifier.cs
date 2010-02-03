#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

using System.Diagnostics;
using System.Text.RegularExpressions;

#endregion

namespace Sentinel.Classifying
{

    #region Using directives

    #endregion

    /// <summary>
    /// Log message classifier that works by processing the description field.
    /// </summary>
    public class DescriptionClassifier : VisualClassifier, IDescriptionClassifier
    {
        private string regexString;

        private Regex regularExpression;

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

        public DescriptionClassifierRecord Classify(string input)
        {
            Debug.Assert(Enabled, "Should not be attempting to classify using non-enabled classifiers.");
            if (regularExpression != null)
            {
                Match m = regularExpression.Match(input);
                if (m.Success)
                {
                    string system = m.Groups["system"].Value;
                    string description = m.Groups["description"].Value;
                    return new DescriptionClassifierRecord(system, description);
                }
            }

            return null;
        }

        #endregion
    }
}