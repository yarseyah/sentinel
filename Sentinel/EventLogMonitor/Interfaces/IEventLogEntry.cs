namespace Sentinel.EventLogMonitor.Interfaces
{
    using System;
    using System.Diagnostics;

    using Newtonsoft.Json;

    public interface IEventLogEntry
    {
        [JsonProperty(Order = 1, Required = Required.Always)]
        EventLogEntryType EntryType { get; }

        [JsonProperty(Order = 2)]
        string Message { get; }

        [JsonProperty(Order = 3)]
        string Source { get; }

        [JsonProperty(Order = 4)]
        DateTime EventTime { get; }

        [JsonProperty(Order = 5)]
        string Category { get; }

        [JsonProperty(Order = 6, DefaultValueHandling = DefaultValueHandling.Ignore)]
        long InstanceId { get; }

        [JsonProperty(Order = 7, DefaultValueHandling = DefaultValueHandling.Ignore)]
        string MachineName { get; }

        [JsonProperty(Order = 8, DefaultValueHandling = DefaultValueHandling.Ignore)]
        string UserName { get; }
    }
}