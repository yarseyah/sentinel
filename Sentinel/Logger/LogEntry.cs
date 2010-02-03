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

#endregion

namespace Sentinel.Logger
{
    public class LogEntry : ILogEntry
    {
        public string Host { get; set; }

        #region ILogEntry Members

        public string Classification { get; set; }

        public DateTime DateTime { get; set; }

        public string Description { get; set; }

        public string Source { get; set; }

        public string System { get; set; }

        public string Thread { get; set; }

        public string Type { get; set; }

        #endregion
    }
}