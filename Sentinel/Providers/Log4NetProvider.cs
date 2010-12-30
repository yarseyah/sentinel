using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Sentinel.Interfaces;
using Sentinel.Providers.Interfaces;

namespace Sentinel.Providers
{
    public class Log4NetProvider : NetworkBatchingProvider
    {
        public static readonly Guid Id = new Guid("7b5700a4-ad84-430d-b495-8bb9bf257e90");

        public readonly static IProviderInfo Info = new ProviderInfo(
            Id, 
            "Log4Net Network Provider",
            "Handler for the remote side of log4net's UdpAppender.");

        public Log4NetProvider(IProviderSettings settings)
            : base(settings)
        {
        }

        public override IProviderInfo Information { get { return Info; } }

        protected override LogEntry DecodeEntry(string logEntry)
        {
            XNamespace log4Net = "unique";
            string message = string.Format(
                @"<entry xmlns:log4net=""{0}"">{1}</entry>",
                log4Net,
                logEntry);

            XElement element = XElement.Parse(message);
            XElement record = element.Element(log4Net + "event");

            // Establish whether a sub-system seems to be defined.
            string description = record.Element(log4Net + "message").Value;

            string classification = String.Empty;
            string system = record.Attribute("logger").Value;
            string type = record.Attribute("level").Value;
            string host = "???";

            foreach (XElement propertyElement in record.Element(log4Net + "properties").Elements())
            {
                if (propertyElement.Name == log4Net + "data" && propertyElement.Attribute("name") != null)
                {
                    host = propertyElement.Attribute("value").Value;
                }
            }

            return new LogEntry
                       {
                           DateTime = DateTime.Parse(record.Attribute("timestamp").Value),
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

        protected override bool IsValidEntry(string message)
        {
            return message.StartsWith("<log4net");
        }
    }
}