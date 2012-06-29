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
            Highlighter highlighter = null;

            Debug.Assert(
                data.MatchField >= 0,
                "Field selected must be a valid selection, e.g. >= 0");
            Debug.Assert(
                data.MatchField < data.MatchFields.Count(),
                "Field selected must be within the available set of fields.");

            Debug.Assert(
                data.HighlightingMethod >= 0,
                "Method must be a valid selection, >= 0");
            Debug.Assert(
                data.HighlightingMethod < data.HighlightingMethods.Count(),
                "Method selected must be with the available set of methods.");

            var mode = MatchModeConverter.ConvertFrom(data.HighlightingMethods.ElementAt(data.HighlightingMethod));

            Color? background = null;
            Color? foreground = null;

            if (data.MatchField >= 0 && data.MatchField < data.MatchFields.Count())
            {
                LogEntryField fieldToMatch = LogEntryFieldHelper.FieldNameToEnumeration(
                    data.MatchFields.ElementAt(data.MatchField));

                switch (fieldToMatch)
                {
                    case LogEntryField.Type:
                    case LogEntryField.System:
                        highlighter = new Highlighter
                                          {
                                              Name = data.Name,
                                              Field = fieldToMatch,
                                              Pattern = data.Pattern,
                                              Mode = mode,
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
                        break;
                }
            }

            return highlighter;
        }
    }
}