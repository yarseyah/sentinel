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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Sentinel.Logs.Interfaces;
using Sentinel.Support.Mvvm;

#endregion

namespace Sentinel.Logs
{
    public class LogManager : ViewModelBase, ILogManager
    {
        private readonly Dictionary<string, ILogger> loggers = new Dictionary<string, ILogger>();

        #region ILogManager Members

        public ILogger Add(string logName)
        {
            Debug.Assert(!string.IsNullOrEmpty(logName), "Log name can not be null or empty.");
            if ( string.IsNullOrEmpty(logName) )
            {
                throw new ArgumentException(
                    "Log name can not be null or empty for LogManager.Add(...)",
                    logName);
            }

            Debug.Assert(!loggers.ContainsKey(logName), "Log name has already been used.");
            if ( loggers.ContainsKey(logName) )
            {
                throw new ArgumentException(
                    "LogManager does not support duplicate log names.",
                    "logName");
            }

            Log log = new Log {Name = logName};

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

        #endregion

        #region Implementation of IEnumerable

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/>
        /// that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
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
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}