using System.Collections.Generic;
using System.ComponentModel;
using Sentinel.Interfaces;

namespace Sentinel.Logs.Interfaces
{
    /// <summary>
    /// Interface for a representation of a logger.
    /// </summary>
    public interface ILogger : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the entries for the logger.
        /// </summary>
        IEnumerable<LogEntry> Entries { get; }

        /// <summary>
        /// Gets the name of the logger.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets the newly added entries for the logger.
        /// </summary>
        IEnumerable<LogEntry> NewEntries { get; }

        /// <summary>
        /// Clear the log entries.
        /// </summary>
        void Clear();

        /// <summary>
        /// Add a batch of new messages to the logger.
        /// </summary>
        /// <param name="entries">Ordered list/queue of items to add.</param>
        void AddBatch(Queue<LogEntry> entries);
    }
}
