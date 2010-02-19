#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

using System.Collections.Generic;

#endregion

namespace Sentinel.Logger
{
    /// <summary>
    /// Interface for a representation of a logger.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Gets the entries for the logger.
        /// </summary>
        IEnumerable<ILogEntry> Entries { get; }

        /// <summary>
        /// Gets the name of the logger.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the newly added entries for the logger.
        /// </summary>
        IEnumerable<ILogEntry> NewEntries { get; }

        /// <summary>
        /// Add a batch of new messages to the logger.
        /// </summary>
        /// <param name="queue">Ordered list/queue of items to add.</param>
        void AddBatch(Queue<string> queue);

        /// <summary>
        /// Clear the log entries.
        /// </summary>
        void Clear();
    }
}