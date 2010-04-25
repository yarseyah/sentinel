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
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using Sentinel.Highlighters.Interfaces;
using Sentinel.Interfaces;

#endregion

namespace Sentinel.Highlighters
{

    #region Using directives

    #endregion

    public class HighlighterConverter : IValueConverter
    {
        private readonly IHighlighter highlighter;

        public HighlighterConverter(IHighlighter highlighter)
        {
            this.highlighter = highlighter;
        }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.Assert(value is LogEntry, "Supplied value must be a LogEntry for conversion.");
            return highlighter.IsMatch(value as LogEntry) && highlighter.Enabled
                       ? "Match"
                       : "Not Match";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}