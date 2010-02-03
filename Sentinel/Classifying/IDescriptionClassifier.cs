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
    /// IDescriptionClassifier is the interface for a classification
    /// instance that works upon a description string.  This may require
    /// parsing the text to come up with an altered classification to that
    /// originally found.
    /// </summary>
    public interface IDescriptionClassifier : IClassifier
    {
        DescriptionClassifierRecord Classify(string input);
    }
}