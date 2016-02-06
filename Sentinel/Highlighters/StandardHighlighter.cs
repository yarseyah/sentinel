namespace Sentinel.Highlighters
{
    using Interfaces;
    using Sentinel.Interfaces;

    public class StandardHighlighter : Highlighter, IStandardDebuggingHighlighter
    {
        public StandardHighlighter(string name, bool enabled, LogEntryField field, MatchMode mode, string pattern, IHighlighterStyle style)
            : base(name, enabled, field, mode, pattern, style)
        {
        }
    }
}