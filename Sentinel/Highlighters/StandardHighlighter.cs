namespace Sentinel.Highlighters
{
    using Sentinel.Highlighters.Interfaces;
    using Sentinel.Interfaces;

    public class StandardHighlighter : Highlighter, IStandardDebuggingHighlighter
    {
        public StandardHighlighter(string name, bool enabled, LogEntryFields field, MatchMode mode, string pattern, IHighlighterStyle style)
            : base(name, enabled, field, mode, pattern, style)
        {
        }
    }
}