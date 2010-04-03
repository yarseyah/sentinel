#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Linq;
using Sentinel.Classifying;
using Sentinel.Services;
using Sentinel.Support;

#endregion

namespace Sentinel.Logger
{

    #region Using directives

    #endregion

    public class Log : ViewModelBase, ILogger
    {
        private readonly IClassifierService classifier;

        private string name;

        private readonly static DateTime log4jDateBase = new DateTime(1970, 1, 1);

        public Log(string name)
        {
            Name = name;

            Entries = new List<ILogEntry>();
            NewEntries = new List<ILogEntry>();

            // Observe the NewEntries to maintain a full history.
            PropertyChanged += OnPropertyChanged;

            classifier = ServiceLocator.Instance.Get<IClassifierService>();
        }

        #region ILogger Members

        public IEnumerable<ILogEntry> Entries { get; private set; }

        public string Name
        {
            get
            {
                return name;
            }

            private set
            {
                if (value != name)
                {
                    name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public IEnumerable<ILogEntry> NewEntries { get; private set; }

        public void AddBatch(Queue<string> pendingQueue)
        {
            List<ILogEntry> newList = new List<ILogEntry>();

            while (pendingQueue.Count > 0)
            {
                string queuedMessage = pendingQueue.Dequeue();

                ILogEntry newEntry = null;
                // Need to identify the message.
                if ( queuedMessage.StartsWith("<log4j"))
                {
                    newEntry = AddLog4jMessage(queuedMessage);
                }
                else if ( queuedMessage.StartsWith("<log4net"))
                {
                    newEntry = AddLog4NetMessage(queuedMessage);
                }
                else
                {
                    throw new NotSupportedException("Don't know what format this message is in!\r\n" + queuedMessage);
                }

                newList.Add(newEntry);
            }

            lock (NewEntries) NewEntries = newList;
            OnPropertyChanged("NewEntries");
        }

        public void Clear()
        {
            lock (Entries)
            {
                ((IList<ILogEntry>) Entries).Clear();
            }

            OnPropertyChanged("Entries");

            lock (NewEntries)
            {
                ((IList<ILogEntry>) NewEntries).Clear();
            }

            GC.Collect();
        }

        #endregion

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "NewEntries")
            {
                lock (NewEntries)
                {
                    lock (Entries)
                    {
                        ((List<ILogEntry>) Entries).AddRange(NewEntries);
                    }

                    OnPropertyChanged("Entries");
                }
            }
        }

        private ILogEntry AddLog4jMessage(string m)
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

            description = ClassifyMessage(description, ref system, ref classification, ref type);

            DateTime date = log4jDateBase + TimeSpan.FromMilliseconds(Double.Parse(record.Attribute("timestamp").Value));

            return new LogEntry
            {
                DateTime = date,
                System = system,
                Classification = classification,
                Thread = record.Attribute("thread").Value,
                Description = description,
                Type = type,
                Host = host
            };
        }

        private ILogEntry AddLog4NetMessage(string m)
        {
            XNamespace log4Net = "unique";
            string message = string.Format(
                @"<entry xmlns:log4net=""{0}"">{1}</entry>",
                log4Net,
                m);

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

            description = ClassifyMessage(description, ref system, ref classification, ref type);

            return new LogEntry
            {
                DateTime = DateTime.Parse(record.Attribute("timestamp").Value),
                System = system,
                Classification = classification,
                Thread = record.Attribute("thread").Value,
                Description = description,
                Type = type,
                Host = host
            };
        }

        private string ClassifyMessage(string description, ref string system, ref string classification, ref string type)
        {
            if (classifier != null)
            {
                foreach (IClassifier c in classifier.Items)
                {
                    if (c.Enabled)
                    {
                        if (c is IDescriptionClassifier)
                        {
                            DescriptionClassifierRecord classifierRecord =
                                (c as IDescriptionClassifier).Classify(description);

                            if (classifierRecord != null)
                            {
                                system = classifierRecord.System;
                                description = classifierRecord.Description;
                                classification = c.Name;
                                break;
                            }
                        }
                        else if (c is IDescriptionTypeClassifier)
                        {
                            DescriptionTypeClassifierRecord classifierRecord =
                                (c as IDescriptionTypeClassifier).Classify(description);

                            if (classifierRecord != null)
                            {
                                type = classifierRecord.Type;
                                description = classifierRecord.Description;
                                classification = c.Name;
                                break;
                            }
                        }
                    }
                }
            }
            return description;
        }

    }
}