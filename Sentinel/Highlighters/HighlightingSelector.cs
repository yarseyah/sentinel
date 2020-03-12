namespace Sentinel.Highlighters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;
    using Sentinel.Highlighters.Interfaces;
    using Sentinel.Interfaces;
    using Sentinel.Interfaces.CodeContracts;
    using Sentinel.Services;
    using Sentinel.Support.Wpf;

    /// <summary>
    /// Style selector that provides a implements the highlighters of the QuickHighlighter
    /// (fancy name for the highlighting of results for the search box) and other registered
    /// highlighters.  This class gets disposed of and rebuilt from scratch when the constituent
    /// highlighters change their status.
    /// </summary>
    public class HighlightingSelector : StyleSelector
    {
        private readonly Dictionary<IHighlighter, Style> styles = new Dictionary<IHighlighter, Style>();

        /// <summary>
        /// Initializes a new instance of the <see cref="HighlightingSelector"/> class.
        /// </summary>
        /// <param name="messagesOnMouseDoubleClick">Action to perform on double click.</param>
        public HighlightingSelector(Action<object, MouseButtonEventArgs> messagesOnMouseDoubleClick)
        {
            MessagesOnMouseDoubleClick = messagesOnMouseDoubleClick;
            var oldState = ServiceLocator.Instance.ReportErrors;
            ServiceLocator.Instance.ReportErrors = false;
            var searchHighlighter = ServiceLocator.Instance.Get<ISearchHighlighter>();
            ServiceLocator.Instance.ReportErrors = oldState;

            if (searchHighlighter != null && searchHighlighter.Highlighter.Enabled)
            {
                var highlighter = searchHighlighter.Highlighter;

                var style = new Style(typeof(ListViewItem));

                var trigger = new DataTrigger
                                  {
                                      Binding = new Binding
                                                    {
                                                        ConverterParameter = highlighter,
                                                        Converter = new HighlighterConverter(highlighter),
                                                        Mode = BindingMode.OneWay,
                                                    },
                                      Value = "Match",
                                  };

                if (highlighter.Style != null)
                {
                    if (highlighter.Style.Background != null)
                    {
                        trigger.Setters.Add(
                            new Setter(
                                Control.BackgroundProperty, new SolidColorBrush((Color)highlighter.Style.Background)));
                    }

                    if (highlighter.Style.Foreground != null)
                    {
                        trigger.Setters.Add(
                            new Setter(
                                Control.ForegroundProperty, new SolidColorBrush((Color)highlighter.Style.Foreground)));
                    }
                }

                style.Triggers.Add(trigger);
                SetStyleSpacing(style);

                // TODO: make this optional based upon the settings (but will need to rebuild highlighters when the setting changes.
                RegisterDoubleClickEvent(style, messagesOnMouseDoubleClick);

                styles[highlighter] = style;
            }

            var highlightingService = ServiceLocator.Instance.Get<IHighlightingService<IHighlighter>>();

            if (highlightingService != null)
            {
                foreach (var highlighter in highlightingService.Highlighters)
                {
                    if (highlighter != null)
                    {
                        // No need to create a style if not enabled, should the status of a highlighter
                        // change, then this collection will be rebuilt.
                        if (highlighter.Enabled)
                        {
                            var style = new Style(typeof(ListViewItem));

                            var trigger = new DataTrigger
                            {
                                Binding =
                                    new Binding
                                    {
                                        ConverterParameter = highlighter,
                                        Converter = new HighlighterConverter(highlighter),
                                        Mode = BindingMode.OneWay,
                                    },
                                Value = "Match",
                            };

                            if (highlighter.Style != null)
                            {
                                if (highlighter.Style.Background != null)
                                {
                                    trigger.Setters.Add(
                                        new Setter(
                                            Control.BackgroundProperty,
                                            new SolidColorBrush((Color)highlighter.Style.Background)));
                                }

                                if (highlighter.Style.Foreground != null)
                                {
                                    trigger.Setters.Add(
                                        new Setter(
                                            Control.ForegroundProperty,
                                            new SolidColorBrush((Color)highlighter.Style.Foreground)));
                                }
                            }

                            // Top align values
                            style.Setters.Add(
                                new Setter(
                                    Control.VerticalContentAlignmentProperty,
                                    VerticalAlignment.Top));

                            style.Triggers.Add(trigger);

                            SetStyleSpacing(style);

                            // TODO: make this optional based upon the settings (but will need to rebuild highlighters when the setting changes.
                            RegisterDoubleClickEvent(style, messagesOnMouseDoubleClick);
                            styles[highlighter] = style;
                        }
                    }
                }
            }
        }

        private Action<object, MouseButtonEventArgs> MessagesOnMouseDoubleClick { get; }

        /// <summary>
        /// Override of the <c>SelectStyle</c> method.  Looks up a suitable style for the
        /// specified item.
        /// </summary>
        /// <param name="item">Item to use when deciding which style to use.</param>
        /// <param name="container">Container making the request.</param>
        /// <returns>Style to use for displaying of item.</returns>
        public override Style SelectStyle(object item, DependencyObject container)
        {
            var entry = item as ILogEntry;
            if (entry != null)
            {
                foreach (var pair in styles.Where(pair => pair.Key.Enabled).Where(pair => pair.Key.IsMatch(entry)))
                {
                    return pair.Value;
                }
            }

            var defaultStyle = new Style(typeof(ListViewItem));

            Debug.Assert(defaultStyle != null, "Should always get a default style");
            SetStyleSpacing(defaultStyle);
            RegisterDoubleClickEvent(defaultStyle, MessagesOnMouseDoubleClick);
            defaultStyle.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Top));

            return defaultStyle;
        }

        /// <summary>
        /// When the user has selected to compensate for Aero style spacing between
        /// elements, make sure that the style includes this adjustment.
        /// </summary>
        /// <param name="style">Style to adjust spacing, if necessary.</param>
        private static void SetStyleSpacing(Style style)
        {
            var preferences = ServiceLocator.Instance.Get<IUserPreferences>();

            if (preferences != null && preferences.UseTighterRows &&
                ThemeInfo.CurrentThemeFileName == "Aero")
            {
                style.Setters.Add(
                    new Setter(
                        FrameworkElement.MarginProperty,
                        new Thickness(0, -1, 0, -1)));
            }
        }

        private void RegisterDoubleClickEvent(Style style, Action<object, MouseButtonEventArgs> handler)
        {
            style.ThrowIfNull(nameof(style));
            style.Setters.Add(new EventSetter(Control.MouseDoubleClickEvent, new MouseButtonEventHandler(handler)));
        }
    }
}