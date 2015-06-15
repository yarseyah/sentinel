namespace Sentinel.Highlighters.Interfaces
{
    using System.Collections.Generic;

    using Sentinel.Interfaces;

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