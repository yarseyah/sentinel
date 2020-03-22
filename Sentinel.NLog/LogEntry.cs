namespace Sentinel.NLog
{
    using System;
    using System.Collections.Generic;

    using Sentinel.Interfaces;

    public class LogEntry : ILogEntry
    {
        public LogEntry()
        {
            Type = "DEBUG";
            DateTime = DateTime.Now;
            Description = "Fake Message";
            Source = "Fake source";
            System = "System";
            Thread = "123";
        }

        /// <summary>
        /// Classification for the log entry.  Can be free-text but will typically
        /// contain values like "DEBUG" or "ERROR".
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Date/Time for the original log entry.
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// The main body of the log entry.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Source of the log entry, e.g. where it came from.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// The system (e.g. machine) where this message came from.
        /// </summary>
        public string System { get; set; }

        /// <summary>
        /// Thread identifier for the source of the message.
        /// </summary>
        public string Thread { get; set; }

        /// <summary>
        /// Dictionary of any meta-data that doesn't fit into the above values.
        /// </summary>
        public Dictionary<string, object> MetaData { get; set; }
    }
}