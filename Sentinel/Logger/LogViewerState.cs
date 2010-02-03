#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

namespace Sentinel.Logger
{
    /// <summary>
    /// States for a log viewer's execution state.
    /// </summary>
    public enum LogViewerState
    {
        /// <summary>
        /// Normal operation.
        /// </summary>
        Normal,

        /// <summary>
        /// Paused, processing of incoming data is deferred.
        /// </summary>
        Paused,

        /// <summary>
        /// Stopped, processing of incoming data is suppressed.
        /// </summary>
        Stopped,

        /// <summary>
        /// Unknown state, not a normal state!
        /// </summary>
        Unknown
    }
}