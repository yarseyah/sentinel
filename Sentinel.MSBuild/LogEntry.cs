namespace Sentinel.MSBuild
{
    using System;
    using System.Collections.Generic;

    using Newtonsoft.Json.Linq;

    using Sentinel.Interfaces;
    using Sentinel.Interfaces.CodeContracts;

    internal class LogEntry : ILogEntry
    {
        public LogEntry()
        {
        }

        public LogEntry(string msbuildEventType, JObject content)
        {
            if (string.IsNullOrWhiteSpace(msbuildEventType))
            {
                throw new ArgumentNullException(nameof(msbuildEventType));
            }

            content.ThrowIfNull(nameof(content));

            switch (msbuildEventType)
            {
                case "ErrorRaised":
                    Type = "ERROR";
                    break;
                case "WarningRaised":
                    Type = "WARN";
                    break;
                default:
                    Type = "INFO";
                    break;
            }

            Description = (string)content["Message"];
            DateTime = (DateTime)content["Timestamp"];
            Thread = ((int)content["ThreadId"]).ToString();
            Source = (string)content["SenderName"];
            System = msbuildEventType;

            if (Description.ToUpper().Contains("EXCEPTION"))
            {
                MetaData.Add("Exception", true);
            }

            MetaData = new Dictionary<string, object> { { "Original", content } };
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