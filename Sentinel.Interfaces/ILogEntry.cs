namespace Sentinel.Interfaces
{
    using System;
    using System.Collections.Generic;

    public interface ILogEntry
    {
        /// <summary>
        /// Classification for the log entry.  Can be free-text but will typically
        /// contain values like "DEBUG" or "ERROR".
        /// </summary>
        string Type { get; set; }

        /// <summary>
        /// Date/Time for the original log entry.
        /// </summary>
        DateTime DateTime { get; set; }

        /// <summary>
        /// The main body of the log entry.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Source of the log entry, e.g. where it came from.
        /// </summary>
        string Source { get; set; }

        /// <summary>
        /// The system (e.g. machine) where this message came from.
        /// </summary>
        string System { get; set; }

        /// <summary>
        /// Thread identifier for the source of the message.
        /// </summary>
        string Thread { get; set; }

        /// <summary>
        /// Dictionary of any meta-data that doesn't fit into the above values.
        /// </summary>
        Dictionary<string, object> MetaData { get; set; }
    }
}