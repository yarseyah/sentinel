namespace Sentinel.Filters
{
    using Sentinel.Filters.Interfaces;
    using Sentinel.Interfaces;

    public class StandardFilter : Filter, IStandardDebuggingFilter
    {
        public StandardFilter(string name, LogEntryFields field, string pattern)
            : base(name, field, pattern)
        {
        }
    }
}