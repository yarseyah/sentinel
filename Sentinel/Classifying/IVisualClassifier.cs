#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

using Sentinel.Highlighting;

#endregion

namespace Sentinel.Classifying
{

    #region Using directives

    #endregion

    public interface IVisualClassifier : IClassifier
    {
        HighlighterStyle Style { get; set; }
    }
}