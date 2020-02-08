namespace Sentinel.Interfaces
{
    using System;
    using System.Runtime.Serialization;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// An enumeration of the possible fields within a LogEntry so that
    /// a reference can be made to a specific property, for example,
    /// when implementing a customisable filter, it will be possible to
    /// indicate which field the filter applies to without using free-text
    /// or multiple overloaded methods.
    /// </summary>
    [Flags]
    [DataContract]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LogEntryFields
    {
        /// <summary>
        /// Not a field enumeration, default.
        /// </summary>
        None = 0,

        /// <summary>
        /// Type field, e.g. ERROR, DEBUG.
        /// </summary>
        Type = 1,

        /// <summary>
        /// System field of the log entry, e.g. "LogManager" or "ConfigurationService"
        /// </summary>
        System = 2,

        /// <summary>
        /// Classification of log entry, used internally when messages get reclassified
        /// or attributed from a specific source.
        /// </summary>
        Classification = 4,

        /// <summary>
        /// Thread identifier of original message.
        /// </summary>
        /// <remarks>
        /// Not the most useful field but since this a string, not a number, it can
        /// be used for overloaded behaviour in the future.
        /// </remarks>
        Thread = 8,

        /// <summary>
        /// Source field.
        /// </summary>
        Source = 16,

        /// <summary>
        /// Description field, this is usually the log message sent from the source.
        /// </summary>
        /// <remarks>
        /// Some of the incoming classification mechanisms may have preprocessed
        /// this field to retrieve and remove certain information so that the presented
        /// description has been 'cracked' into constituent parts.
        /// </remarks>
        Description = 32,

        /// <summary>
        /// Host field of message, usually the originating machine's name.
        /// </summary>
        Host = 64,
    }
}