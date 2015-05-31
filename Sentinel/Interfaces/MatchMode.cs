#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

namespace Sentinel.Interfaces
{
    using System.Runtime.Serialization;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Modes for matching strings.
    /// </summary>
    [DataContract]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MatchMode
    {
        /// <summary>
        /// Exact case-sensitive match.
        /// </summary>
        Exact,

        /// <summary>
        /// Case-sensitive sub-string match.
        /// </summary>
        CaseSensitive,

        /// <summary>
        /// Case-insensitive sub-string match.
        /// </summary>
        CaseInsensitive,

        /// <summary>
        /// Regular expression matching.
        /// </summary>
        RegularExpression
    }
}