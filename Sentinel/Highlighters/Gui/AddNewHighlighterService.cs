#region License
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
#endregion

namespace Sentinel.Highlighters.Gui
{
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;

    using Sentinel.Highlighters.Interfaces;
    using Sentinel.Interfaces;
    using Sentinel.Services;
    using Sentinel.Support.Converters;

    public class AddNewHighlighterService : IAddHighlighterService
    {
        #region IAddHighlighterService Members

        public void Add()
        {
            var window = new AddEditHighlighterWindow();
            var data = new AddEditHighlighter(window, false);
            window.DataContext = data;
            window.Owner = Application.Current.MainWindow;

            var dialogResult = window.ShowDialog();
            if (dialogResult != null && (bool)dialogResult)
            {
                var highlighter = Construct(data);

                if (highlighter != null)
                {
                    var service = ServiceLocator.Instance.Get<IHighlightingService<IHighlighter>>();
                    if (service != null)
                    {
                        service.Highlighters.Add(highlighter);
                    }
                }
            }
        }

        #endregion

        public Highlighter Construct(AddEditHighlighter data)
        {
            Color? background = null;
            Color? foreground = null;

            var highlighter = new Highlighter
                              {
                                  Name = data.Name,
                                  Field = data.Field,
                                  Pattern = data.Pattern,
                                  Mode = data.Mode,
                                  Enabled = true
                              };

            if (data.OverrideBackgroundColour)
            {
                background = data.BackgroundColour;
            }

            if (data.OverrideForegroundColour)
            {
                foreground = data.ForegroundColour;
            }

            highlighter.Style = new HighlighterStyle
                                    {
                                        Background = background,
                                        Foreground = foreground
                                    };
            return highlighter;
        }
    }
}