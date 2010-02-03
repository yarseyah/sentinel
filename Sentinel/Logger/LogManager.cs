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
using Sentinel.Support;

#endregion

namespace Sentinel.Logger
{

    #region Using directives

    #endregion

    public class LogManager : ViewModelBase, ILogManager
    {
        private readonly Dictionary<string, ILogger> loggers = new Dictionary<string, ILogger>();

        #region ILogManager Members

        public void Add(ILogger logger)
        {
            if (logger != null)
            {
                if (!loggers.ContainsKey(logger.Name))
                {
                    loggers[logger.Name] = logger;
                }
                else
                {
                    throw new ArgumentException("Duplicates are not supported");
                }
            }
            else
            {
                throw new ArgumentNullException("logger");
            }
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
    }
}