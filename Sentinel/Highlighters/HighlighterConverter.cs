namespace Sentinel.Highlighters
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Windows.Data;

    using Sentinel.Highlighters.Interfaces;
    using Sentinel.Interfaces;

    public class HighlighterConverter : IValueConverter
    {
        private readonly IHighlighter highlighter;

        public HighlighterConverter(IHighlighter highlighter)
        {
            this.highlighter = highlighter;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.Assert(value is ILogEntry, "Supplied value must be a LogEntry for conversion.");
            return highlighter.Enabled && highlighter.IsMatch(value as ILogEntry)
                       ? "Match"
                       : "Not Match";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}