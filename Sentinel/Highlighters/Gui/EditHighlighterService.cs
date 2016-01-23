namespace Sentinel.Highlighters.Gui
{
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Media;

    using Sentinel.Highlighters.Interfaces;

    // [Export(typeof(IEditHighlighterService))]
    public class EditHighlighterService : IEditHighlighterService
    {
        public void Edit(IHighlighter highlighter)
        {
            Debug.Assert(highlighter != null, "Highlighter must be supplied for editing.");

            var window = new AddEditHighlighterWindow();
            var data = new AddEditHighlighter(window, false);
            window.DataContext = data;
            window.Owner = Application.Current.MainWindow;

            data.Name = highlighter.Name;
            data.Pattern = highlighter.Pattern;
            data.Mode = highlighter.Mode;
            data.Field = highlighter.Field;

            if (highlighter.Style?.Background != null)
            {
                data.OverrideBackgroundColour = true;
                data.BackgroundColour = (Color)highlighter.Style.Background;
            }
            else
            {
                data.OverrideBackgroundColour = false;
                data.BackgroundColourIndex = 1;
            }

            if (highlighter.Style?.Foreground != null)
            {
                data.OverrideForegroundColour = true;
                data.ForegroundColour = (Color)highlighter.Style.Foreground;
            }
            else
            {
                data.OverrideForegroundColour = false;
                data.ForegroundColourIndex = 0;
            }

            var dialogResult = window.ShowDialog();

            if (dialogResult != null && (bool)dialogResult)
            {
                highlighter.Name = data.Name;
                highlighter.Pattern = data.Pattern;
                highlighter.Mode = data.Mode;
                highlighter.Field = data.Field;

                if (highlighter.Style == null && (data.OverrideBackgroundColour || data.OverrideForegroundColour))
                {
                    highlighter.Style = new HighlighterStyle();
                }

                if (highlighter.Style != null)
                {
                    highlighter.Style.Background = data.OverrideBackgroundColour
                                                       ? (Color?)data.BackgroundColour
                                                       : null;
                    highlighter.Style.Foreground = data.OverrideForegroundColour
                                                       ? (Color?)data.ForegroundColour
                                                       : null;
                }
            }
        }
    }
}