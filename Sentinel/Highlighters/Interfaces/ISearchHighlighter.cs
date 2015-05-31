#region License
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
#endregion

namespace Sentinel.Highlighters.Interfaces
{
    #region Using directives

    using System.Collections;
    using System.Collections.Generic;

    using Sentinel.Interfaces;

    #endregion

    public interface ISearchHighlighter
    {
        IEnumerable<LogEntryField> Fields { get; }

        LogEntryField Field { get; set; }

        bool Enabled { get; set; }

        MatchMode Mode { get; set; }

        IHighlighter Highlighter { get; }

        string Search { get; set; }
    }
}