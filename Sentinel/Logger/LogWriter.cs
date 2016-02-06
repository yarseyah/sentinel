namespace Sentinel.Logger
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows;
    using Sentinel.Interfaces;

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
                    foreach (var entry in entries)
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