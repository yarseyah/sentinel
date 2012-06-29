namespace Sentinel.Highlighters.Interfaces
{
    using System.Runtime.Serialization;

    using Sentinel.Interfaces;

    public interface IHighlighter
    {
        [DataMember]
        string Name { get; set; }

        [DataMember]
        bool Enabled { get; set; }

        [DataMember]
        LogEntryField Field { get; set; }

        [DataMember]
        MatchMode Mode { get; set; }

        [DataMember]
        string Pattern { get; set; }

        [DataMember]
        HighlighterStyle Style { get; set; }

        bool IsMatch(LogEntry logEntry);
    }
}