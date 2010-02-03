#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

namespace Sentinel.Classifying
{
    /// <summary>
    /// IDescriptionTypeClassifier is the interface for a classification
    /// instance that works upon a description string and suggests a new
    /// type string if <code>IsMatch</code> would return true.
    /// </summary>
    public interface IDescriptionTypeClassifier : IClassifier
    {
        string AlternativeType { get; }

        DescriptionTypeClassifierRecord Classify(string input);
    }
}