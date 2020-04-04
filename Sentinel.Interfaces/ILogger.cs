namespace Sentinel.Interfaces
{
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    /// Interface for a representation of a logger.
    /// </summary>
    public interface ILogger : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the entries for the logger.
        /// </summary>
        IEnumerable<ILogEntry> Entries { get; }

        /// <summary>
        /// Gets or sets the name of the logger.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether new entries are added to the Entries collection.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Gets the newly added entries for the logger.
        /// </summary>
        IEnumerable<ILogEntry> NewEntries { get; }

        /// <summary>
        /// Clear the log entries.
        /// </summary>
        void Clear();

        /// <summary>
        /// Add a batch of new messages to the logger.
        /// </summary>
        /// <param name="entries">Ordered list/queue of items to add.</param>
        void AddBatch(Queue<ILogEntry> entries);

        /// <summary>
        /// Allows a specific limit of messages to be applied.
        /// Note, it is the responsibility of the appender to enforce limits.
        /// </summary>
        /// <param name="maximumMessages">
        /// Number representing the maximum number of entries required.
        /// </param>
        void LimitMessageCount(int maximumMessages);
    }
}
