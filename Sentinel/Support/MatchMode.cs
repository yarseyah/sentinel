#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

namespace Sentinel.Support
{
    /// <summary>
    /// Modes for matching strings.
    /// </summary>
    public enum MatchMode
    {
        /// <summary>
        /// Exact case-sensitive match.
        /// </summary>
        Exact,

        /// <summary>
        /// Case-sensitive sub-string match.
        /// </summary>
        Substring,

        /// <summary>
        /// Regular expression matching.
        /// </summary>
        RegularExpression
    }
}