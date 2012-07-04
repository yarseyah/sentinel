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
using System.IO;
using System.Windows;
using Sentinel.Interfaces;

#endregion

namespace Sentinel.Logger
{
    /// <summary>
    /// This service provider permits the writing of log entries to
    /// a text file.
    /// </summary>
    public class LogWriter
    {
        /// <summary>
        /// Register the current fields to a default text file.
        /// </summary>
        /// <param name="entries">Entries to write to text file.</param>
        public void Write(IEnumerable<ILogEntry> entries)
        {
            try
            {
                using (TextWriter tw = File.CreateText("log"))
                {
                    foreach (ILogEntry entry in entries)
                    {
                        tw.WriteLine(
                            "{0} {1} [{2}] {3}",
                            entry.DateTime,
                            entry.Type,
                            entry.System,
                            entry.Description);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(
                    "Caught an exception when writing:\r\n\r\n" + e.Message,
                    "Log Writing Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}