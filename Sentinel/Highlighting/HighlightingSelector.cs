#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Sentinel.Logger;
using Sentinel.Preferences;
using Sentinel.Services;
using Sentinel.Support;

#endregion

namespace Sentinel.Highlighting
{

    #region Using directives

    #endregion

    /// <summary>
    /// Style selector that provides a implements the highlighters of the QuickHighligher
    /// (fancy name for the highlighting of results for the search box) and other registered
    /// highlighters.  This class gets disposed of and rebuilt from scratch when the constituent 
    /// highlighters change their status.
    /// </summary>
    public class HighlightingSelector : StyleSelector
    {
        private readonly Dictionary<Highlighter, Style> styles = new Dictionary<Highlighter, Style>();

        /// <summary>
        /// Initializes a new instance of the HighlightingSelector class.
        /// </summary>
        public HighlightingSelector()
        {
            bool oldState = ServiceLocator.Instance.ReportErrors;
            ServiceLocator.Instance.ReportErrors = false;
            IQuickHighlighter quickHighlighter = ServiceLocator.Instance.Get<IQuickHighlighter>();
            ServiceLocator.Instance.ReportErrors = oldState;

            if (quickHighlighter != null && quickHighlighter.Highlighter.Enabled)
            {
                Highlighter highlighter = quickHighlighter.Highlighter;

                Style style = new Style(typeof(ListViewItem));
                DataTrigger trigger = new DataTrigger
                                          {
                                              Binding = new Binding
                                                            {
                                                                ConverterParameter = highlighter,
                                                                Converter = new HighlighterConverter(highlighter),
                                                                Mode = BindingMode.OneWay
                                                            },
                                              Value = "Match"
                                          };

                if (highlighter.Style != null)
                {
                    if (highlighter.Style.Background != null)
                    {
                        trigger.Setters.Add(
                            new Setter(
                                Control.BackgroundProperty,
                                new SolidColorBrush((Color) highlighter.Style.Background)));
                    }

                    if (highlighter.Style.Foreground != null)
                    {
                        trigger.Setters.Add(
                            new Setter(
                                Control.ForegroundProperty,
                                new SolidColorBrush((Color) highlighter.Style.Foreground)));
                    }
                }

                style.Triggers.Add(trigger);
                SetStyleSpacing(style);
                styles[highlighter] = style;
            }

            IHighlightingService highlightingService = ServiceLocator.Instance.Get<IHighlightingService>();

            if (highlightingService != null)
            {
                foreach (Highlighter highlighter in highlightingService.Highlighters)
                {
                    if (highlighter != null)
                    {
                        // No need to create a style if not enabled, should the status of a highlighter
                        // change, then this collection will be rebuilt.
                        if (highlighter.Enabled)
                        {
                            Style style = new Style(typeof(ListViewItem));

                            DataTrigger trigger = new DataTrigger
                                                      {
                                                          Binding = new Binding
                                                                        {
                                                                            ConverterParameter = highlighter,
                                                                            Converter =
                                                                                new HighlighterConverter(highlighter),
                                                                            Mode = BindingMode.OneWay
                                                                        },
                                                          Value = "Match"
                                                      };

                            if (highlighter.Style != null)
                            {
                                if (highlighter.Style.Background != null)
                                {
                                    trigger.Setters.Add(
                                        new Setter(
                                            Control.BackgroundProperty,
                                            new SolidColorBrush((Color) highlighter.Style.Background)));
                                }

                                if (highlighter.Style.Foreground != null)
                                {
                                    trigger.Setters.Add(
                                        new Setter(
                                            Control.ForegroundProperty,
                                            new SolidColorBrush((Color) highlighter.Style.Foreground)));
                                }
                            }

                            style.Triggers.Add(trigger);
                            SetStyleSpacing(style);
                            styles[highlighter] = style;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Override of the <c>SelectStyle</c> method.  Looks up a suitable style for the
        /// specified item.
        /// </summary>
        /// <param name="item">Item to use when deciding which style to use.</param>
        /// <param name="container">Container making the request?</param>
        /// <returns>Style to use for displaying of item.</returns>
        public override Style SelectStyle(object item, DependencyObject container)
        {
            ILogEntry entry = item as ILogEntry;
            if (entry != null)
            {
                foreach (KeyValuePair<Highlighter, Style> pair in styles)
                {
                    if (pair.Key.IsMatch(entry) && pair.Key.Enabled)
                    {
                        return pair.Value;
                    }
                }
            }

            return base.SelectStyle(item, container);
        }

        /// <summary>
        /// When the user has selected to compensate for Aero style spacing between
        /// elements, make sure that the style includes this adjustment.
        /// </summary>
        /// <param name="style">Style to adjust spacing, if necessary.</param>
        private static void SetStyleSpacing(Style style)
        {
            IUserPreferences preferences = ServiceLocator.Instance.Get<IUserPreferences>();

            if (preferences != null && preferences.UseTighterRows &&
                ThemeInfo.CurrentThemeFileName == "Aero")
            {
                style.Setters.Add(
                    new Setter(
                        FrameworkElement.MarginProperty,
                        new Thickness(0, -1, 0, -1)));
            }
        }
    }
}