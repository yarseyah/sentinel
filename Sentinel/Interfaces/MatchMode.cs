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
        RegularExpression,
    }
}