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

    public class DescriptionTypeClassifier
        : VisualClassifier, IDescriptionTypeClassifier
    {
        private readonly string alternativeType;

        private string regexString;

        private Regex regularExpression = null;

        public DescriptionTypeClassifier(string alternativeType, string label)
            : base("Description Type Classifier")
        {
            this.alternativeType = alternativeType;
        }

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
                    regularExpression = (value != string.Empty)
                                            ? new Regex(value, RegexOptions.Compiled)
                                            : null;
                }
            }
        }

        #region IDescriptionTypeClassifier Members

        public string AlternativeType
        {
            get
            {
                return alternativeType;
            }
        }

        public override bool IsMatch(object parameter)
        {
            return regularExpression != null && regularExpression.Match((string) parameter).Success;
        }

        public DescriptionTypeClassifierRecord Classify(string input)
        {
            Debug.Assert(Enabled, "Should not be attempting to classify using non-enabled classifiers.");
            if (regularExpression != null)
            {
                Match m = regularExpression.Match(input);
                if (m.Success)
                {
                    string description = regexString.Contains("(?<description>")
                                             ? m.Groups["description"].Value
                                             : input;

                    DescriptionTypeClassifierRecord record =
                        new DescriptionTypeClassifierRecord(alternativeType, description);
                    return record;
                }

                // If it looks like the regex has a way of overriding the description, use that
                // otherwise use the whole of the input string as the description.
                return null;
            }

            return null;
        }

        #endregion
    }
}