#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Sentinel.Highlighters.Interfaces;
using Sentinel.Interfaces;

#endregion

namespace Sentinel.Highlighters.Gui
{
    //[Export(typeof(IEditHighlighterService))]
    public class EditHighlighterService : IEditHighlighterService
    {
        #region IEditHighlighterService Members

        public void Edit(IHighlighter highlighter)
        {
            Debug.Assert(highlighter != null, "Highligher must be supplied for editing.");

            AddEditHighlighterWindow window = new AddEditHighlighterWindow();
            AddEditHighlighter data = new AddEditHighlighter(window, false);
            window.DataContext = data;
            window.Owner = Application.Current.MainWindow;

            data.Name = highlighter.Name;
            data.Pattern = highlighter.Pattern;
            data.Mode = highlighter.Mode;
            data.Field = highlighter.Field;         

            if (highlighter.Style != null && highlighter.Style.Background != null)
            {
                data.OverrideBackgroundColour = true;
                data.BackgroundColour = (Color)highlighter.Style.Background;
            }
            else
            {
                data.OverrideBackgroundColour = false;
                data.BackgroundColourIndex = 1;
            }

            if (highlighter.Style != null && highlighter.Style.Foreground != null)
            {
                data.OverrideForegroundColour = true;
                data.ForegroundColour = (Color)highlighter.Style.Foreground;
            }
            else
            {
                data.OverrideForegroundColour = false;
                data.ForegroundColourIndex = 0;
            }

            bool? dialogResult = window.ShowDialog();

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

        #endregion
    }
}