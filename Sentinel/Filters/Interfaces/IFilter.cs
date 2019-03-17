namespace Sentinel.Filters.Interfaces
{
    using System.Runtime.Serialization;

    using Sentinel.Interfaces;

    public interface IFilter
    {
        [DataMember]
        string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the filter is enabled.
        /// </summary>
        [DataMember]
        bool Enabled { get; set; }

        [DataMember]
        string Pattern { get; set; }

        [DataMember]
        string Description { get; }

        [DataMember]
        LogEntryFields Field { get; set; }

        [DataMember]
        MatchMode Mode { get; set; }

        bool IsMatch(ILogEntry entry);
    }
}