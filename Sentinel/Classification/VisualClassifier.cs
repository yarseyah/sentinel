#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

using Sentinel.Classification.Interfaces;
using Sentinel.Interfaces;
using Sentinel.Services;

namespace Sentinel.Classification
{
    #region Using directives

    #endregion

    public abstract class VisualClassifier : IVisualClassifier
    {
        /// <summary>
        /// Initializes a new instance of the VisualClassifier class.
        /// </summary>
        /// <param name="type">Type field to operate upon.</param>
        protected VisualClassifier(string type)
        {
            // Implementers of IHighlighterStyle should be set to return a new instance
            // each time, rather than the same one each time!
            Style = ServiceLocator.Instance.Get<IHighlighterStyle>();
            Type = type;
        }

        #region IVisualClassifier Members

        /// <summary>
        /// Gets or sets a value indicating whether the visual classifier is enabled
        /// or not.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the name of the visual classifier.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the <c>HighlighterStyle</c> for the visual classifier.
        /// </summary>
        public IHighlighterStyle Style { get; set; }

        /// <summary>
        /// Gets the type that the classifier operates upon.
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// Determines whether the supplied <paramref name="parameter"/> matches
        /// the classifier's criteria.
        /// </summary>
        /// <param name="parameter">Object to test for matches.</param>
        /// <returns>True if matched.</returns>
        public abstract bool IsMatch(object parameter);

        public abstract LogEntry Classify(LogEntry entry);

        #endregion
    }
}