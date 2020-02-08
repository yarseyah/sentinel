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
        LogEntryFields Field { get; set; }

        [DataMember]
        MatchMode Mode { get; set; }

        [DataMember]
        string Pattern { get; set; }

        [DataMember]
        string Description { get; }

        [DataMember]
        IHighlighterStyle Style { get; set; }

        bool IsMatch(ILogEntry logEntry);
    }
}