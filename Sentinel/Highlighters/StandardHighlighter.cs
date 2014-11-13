namespace Sentinel.Highlighters
{
    using Sentinel.Highlighters.Interfaces;
    using Sentinel.Interfaces;

    public class StandardHighlighter : Highlighter, IStandardDebuggingHighlighter
    {
        public StandardHighlighter()
        {
        }

        public StandardHighlighter(string name, bool enabled, LogEntryField field, MatchMode mode, string pattern, HighlighterStyle style)
            : base(name, enabled, field, mode, pattern, style)
        {
        }
    }
}