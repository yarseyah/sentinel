namespace Sentinel.Logs
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;

    using Sentinel.Interfaces;
    using Sentinel.Interfaces.CodeContracts;
    using Sentinel.Logs.Interfaces;

    using WpfExtras;

    public class LogManager : ViewModelBase, ILogManager
    {
        private readonly Dictionary<string, ILogger> loggers = new Dictionary<string, ILogger>();

        public ILogger Add(string logName)
        {
            logName.ThrowIfNullOrWhiteSpace(nameof(logName));

            Debug.Assert(!loggers.ContainsKey(logName), "Log name has already been used.");
            if (loggers.ContainsKey(logName))
            {
                throw new ArgumentException("LogManager does not support duplicate log names.", nameof(logName));
            }

            var log = new Log { Name = logName };

            loggers[logName] = log;
            return log;
        }

        public ILogger Get(string name)
        {
            return loggers[name];
        }

        public void Remove(string name)
        {
            if (loggers.ContainsKey(name))
            {
                loggers.Remove(name);
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/>
        /// that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1.</filterpriority>
        public IEnumerator<ILogger> GetEnumerator()
        {
            return loggers.Values.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/>
        /// object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2.</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}