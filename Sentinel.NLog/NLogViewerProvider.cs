
namespace Sentinel.NLog
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Linq;

    using Sentinel.Interfaces.Providers;

    public class NLogViewerProvider : NetworkBatchingProvider
    {
        public readonly static Guid Id = new Guid("f12581a5-64c0-4b35-91fc-81c9a09c1e0b");

        private static readonly DateTime log4jDateBase = new DateTime(1970, 1, 1);

        public readonly static ProviderInfo Info = new ProviderInfo(
            Id,
            "nLog Viewer",
            "Handler for nLog's log4j networking protocol log target.");

        public NLogViewerProvider(IProviderSettings settings)
            : base(settings)
        {
        }

        public override IProviderInfo Information { get { return Info; } }

        protected override LogEntry DecodeEntry(string m)
        {
            XNamespace log4j = "unique";
            string message = string.Format(
                @"<entry xmlns:log4j=""{0}"">{1}</entry>",
                log4j,
                m);

            XElement element = XElement.Parse(message);
            XElement record = element.Element(log4j + "event");

            // Establish whether a sub-system seems to be defined.
            string description = record.Element(log4j + "message").Value;

            string classification = String.Empty;
            string system = record.Attribute("logger").Value;
            string type = record.Attribute("level").Value;
            string host = "???";

            foreach (XElement propertyElement in record.Element(log4j + "properties").Elements())
            {
                if (propertyElement.Name == log4j + "data"
                    && propertyElement.Attribute("name") != null
                    && propertyElement.Attribute("name").Value == "log4jmachinename")
                {
                    host = propertyElement.Attribute("value").Value;
                }
            }

            // description = ClassifyMessage(description, ref system, ref classification, ref type);

            DateTime date = log4jDateBase + TimeSpan.FromMilliseconds(Double.Parse(record.Attribute("timestamp").Value));

            return new LogEntry
                       {
                           DateTime = date,
                           System = system,
                           Thread = record.Attribute("thread").Value,
                           Description = description,
                           Type = type,
                           MetaData = new Dictionary<string, object>
                                          {
                                              { "Classification", classification },
                                              { "Host", host }
                                          }
                       };
        }

        protected override bool IsValidEntry(string logEntry)
        {
            return logEntry.StartsWith("<log4j");
        }
    }
}