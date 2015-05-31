using Sentinel.Interfaces;
using System.Runtime.Serialization;

namespace Sentinel.Classification.Interfaces
{
    public interface IClassifier
    {
        [DataMember]
        string Name { get; set; }

        [DataMember]
        string Type { get; set; }

        [DataMember]
        bool Enabled { get; set; }

        [DataMember]
        string Pattern { get; set; }

        [DataMember]
        string Description { get; }

        [DataMember]
        LogEntryField Field { get; set; }

        [DataMember]
        MatchMode Mode { get; set; }

        bool IsMatch(ILogEntry entry);

        ILogEntry Classify(ILogEntry entry);
    }
}