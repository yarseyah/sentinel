namespace Sentinel.Support
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;

    using Sentinel.Interfaces;
    using Sentinel.Interfaces.CodeContracts;

    public static class GridViewSort
    {
        public static readonly DependencyProperty AutoSortProperty;

        public static readonly DependencyProperty CommandProperty;

        public static readonly DependencyProperty PropertyNameProperty =
            DependencyProperty.RegisterAttached(
                "PropertyName",
                typeof(string),
                typeof(GridViewSort),
                new UIPropertyMetadata(null));

        public static readonly DependencyProperty ShowSortGlyphProperty =
            DependencyProperty.RegisterAttached(
                "ShowSortGlyph",
                typeof(bool),
                typeof(GridViewSort),
                new UIPropertyMetadata(true));

        public static readonly DependencyProperty SortGlyphAscendingProperty =
            DependencyProperty.RegisterAttached(
                "SortGlyphAscending",
                typeof(ImageSource),
                typeof(GridViewSort),
                new UIPropertyMetadata(null));

        public static readonly DependencyProperty SortGlyphDescendingProperty =
            DependencyProperty.RegisterAttached(
                "SortGlyphDescending",
                typeof(ImageSource),
                typeof(GridViewSort),
                new UIPropertyMetadata(null));

        private static readonly DependencyProperty SortedColumnHeaderProperty =
            DependencyProperty.RegisterAttached(
                "SortedColumnHeader",
                typeof(GridViewColumnHeader),
                typeof(GridViewSort),
                new UIPropertyMetadata(null));

        static GridViewSort()
        {
            PropertyChangedCallback commandCallback = (o, e) =>
                {
                    ItemsControl listView = o as ItemsControl;
                    if (listView != null)
                    {
                        if (!GetAutoSort(listView))
                        {
                            if (e.OldValue != null && e.NewValue == null)
                            {
                                listView.RemoveHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeaderClick));
                            }

                            if (e.OldValue == null && e.NewValue != null)
                            {
                                listView.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeaderClick));
                            }
                        }
                    }
                };

            CommandProperty = DependencyProperty.RegisterAttached(
                "Command",
                typeof(ICommand),
                typeof(GridViewSort),
                new UIPropertyMetadata(null, commandCallback));

            PropertyChangedCallback autoSortCallback = (o, e) =>
                {
                    ListView listView = o as ListView;
                    if (listView != null)
                    {
                        // Don't change click handler if a command is set
                        if (GetCommand(listView) == null)
                        {
                            bool oldValue = (bool)e.OldValue;
                            bool newValue = (bool)e.NewValue;
                            if (oldValue && !newValue)
                            {
                                listView.RemoveHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeaderClick));
                            }

                            if (!oldValue && newValue)
                            {
                                listView.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeaderClick));
                            }
                        }
                    }
                };

            AutoSortProperty = DependencyProperty.RegisterAttached(
                "AutoSort",
                typeof(bool),
                typeof(GridViewSort),
                new UIPropertyMetadata(false, autoSortCallback));
        }

        public static void ApplySort(
            ICollectionView view,
            string propertyName,
            ListView listView,
            GridViewColumnHeader sortedColumnHeader)
        {
            view.ThrowIfNull(nameof(view));

            var direction = ListSortDirection.Ascending;
            if (view.SortDescriptions.Count > 0)
            {
                SortDescription currentSort = view.SortDescriptions[0];
                if (currentSort.PropertyName == propertyName)
                {
                    direction = currentSort.Direction == ListSortDirection.Ascending
                                    ? ListSortDirection.Descending
                                    : ListSortDirection.Ascending;
                }

                view.SortDescriptions.Clear();

                GridViewColumnHeader currentSortedColumnHeader = GetSortedColumnHeader(listView);
                if (currentSortedColumnHeader != null)
                {
                    RemoveSortGlyph(currentSortedColumnHeader);
                }
            }

            if (!string.IsNullOrEmpty(propertyName))
            {
                view.SortDescriptions.Add(new SortDescription(propertyName, direction));
                if (GetShowSortGlyph(listView))
                {
                    var glyph = direction == ListSortDirection.Ascending
                                    ? GetSortGlyphAscending(listView)
                                    : GetSortGlyphDescending(listView);
                    AddSortGlyph(sortedColumnHeader, direction, glyph);
                }

                SetSortedColumnHeader(listView, sortedColumnHeader);
            }
        }

        [SuppressMessage(
            "Microsoft.Design",
            "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "The generic style registration is desired, despite this rule.")]
        public static T GetAncestor<T>(DependencyObject reference)
            where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(reference);
            while (!(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return (T)parent;
        }

        // Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...
        public static bool GetAutoSort(DependencyObject dependencyObject)
        {
            dependencyObject.ThrowIfNull(nameof(dependencyObject));
            return (bool)dependencyObject.GetValue(AutoSortProperty);
        }

        public static ICommand GetCommand(DependencyObject dependencyObject)
        {
            dependencyObject.ThrowIfNull(nameof(dependencyObject));
            return (ICommand)dependencyObject.GetValue(CommandProperty);
        }

        // Using a DependencyProperty as the backing store for AutoSort.  This enables animation, styling, binding, etc...
        public static string GetPropertyName(DependencyObject dependencyObject)
        {
            dependencyObject.ThrowIfNull(nameof(dependencyObject));
            return (string)dependencyObject.GetValue(PropertyNameProperty);
        }

        // Using a DependencyProperty as the backing store for PropertyName.  This enables animation, styling, binding, etc...
        public static bool GetShowSortGlyph(DependencyObject dependencyObject)
        {
            dependencyObject.ThrowIfNull(nameof(dependencyObject));
            return (bool)dependencyObject.GetValue(ShowSortGlyphProperty);
        }

        // Using a DependencyProperty as the backing store for ShowSortGlyph.  This enables animation, styling, binding, etc...
        public static ImageSource GetSortGlyphAscending(DependencyObject dependencyObject)
        {
            dependencyObject.ThrowIfNull(nameof(dependencyObject));
            return (ImageSource)dependencyObject.GetValue(SortGlyphAscendingProperty);
        }

        // Using a DependencyProperty as the backing store for SortGlyphAscending.  This enables animation, styling, binding, etc...
        public static ImageSource GetSortGlyphDescending(DependencyObject dependencyObject)
        {
            dependencyObject.ThrowIfNull(nameof(dependencyObject));
            return (ImageSource)dependencyObject.GetValue(SortGlyphDescendingProperty);
        }

        public static void SetAutoSort(DependencyObject dependencyObject, bool value)
        {
            dependencyObject.ThrowIfNull(nameof(dependencyObject));
            dependencyObject.SetValue(AutoSortProperty, value);
        }

        public static void SetCommand(DependencyObject dependencyObject, ICommand value)
        {
            dependencyObject.ThrowIfNull(nameof(dependencyObject));
            dependencyObject.SetValue(CommandProperty, value);
        }

        public static void SetPropertyName(DependencyObject dependencyObject, string value)
        {
            dependencyObject.ThrowIfNull(nameof(dependencyObject));
            dependencyObject.SetValue(PropertyNameProperty, value);
        }

        public static void SetShowSortGlyph(DependencyObject dependencyObject, bool value)
        {
            dependencyObject.ThrowIfNull(nameof(dependencyObject));
            dependencyObject.SetValue(ShowSortGlyphProperty, value);
        }

        public static void SetSortGlyphAscending(DependencyObject dependencyObject, ImageSource value)
        {
            dependencyObject.ThrowIfNull(nameof(dependencyObject));
            dependencyObject.SetValue(SortGlyphAscendingProperty, value);
        }

        public static void SetSortGlyphDescending(DependencyObject dependencyObject, ImageSource value)
        {
            dependencyObject.ThrowIfNull(nameof(dependencyObject));
            dependencyObject.SetValue(SortGlyphDescendingProperty, value);
        }

        private static void AddSortGlyph(
            GridViewColumnHeader columnHeader,
            ListSortDirection direction,
            ImageSource sortGlyph)
        {
            var adornerLayer = AdornerLayer.GetAdornerLayer(columnHeader);
            adornerLayer.Add(new SortGlyphAdorner(columnHeader, direction, sortGlyph));
        }

        private static void ColumnHeaderClick(object sender, RoutedEventArgs e)
        {
            var headerClicked = e.OriginalSource as GridViewColumnHeader;
            if (headerClicked != null && headerClicked.Column != null)
            {
                string propertyName = GetPropertyName(headerClicked.Column);
                if (!string.IsNullOrEmpty(propertyName))
                {
                    var listView = GetAncestor<ListView>(headerClicked);
                    if (listView != null)
                    {
                        var command = GetCommand(listView);
                        if (command != null)
                        {
                            if (command.CanExecute(propertyName))
                            {
                                command.Execute(propertyName);
                            }
                        }
                        else if (GetAutoSort(listView))
                        {
                            lock (listView.Items)
                            {
                                ApplySort(listView.Items, propertyName, listView, headerClicked);
                            }
                        }
                    }
                }
            }
        }

        private static GridViewColumnHeader GetSortedColumnHeader(DependencyObject obj)
        {
            return (GridViewColumnHeader)obj.GetValue(SortedColumnHeaderProperty);
        }

        private static void RemoveSortGlyph(GridViewColumnHeader columnHeader)
        {
            var adornerLayer = AdornerLayer.GetAdornerLayer(columnHeader);
            var adornerCollection = adornerLayer.GetAdorners(columnHeader);
            if (adornerCollection != null)
            {
                foreach (var adorner in adornerCollection.OfType<SortGlyphAdorner>())
                {
                    adornerLayer.Remove(adorner);
                }
            }
        }

        private static void SetSortedColumnHeader(DependencyObject obj, GridViewColumnHeader value)
        {
            obj.SetValue(SortedColumnHeaderProperty, value);
        }

        private class SortGlyphAdorner : Adorner
        {
            private readonly GridViewColumnHeader columnHeader;

            private readonly ListSortDirection direction;

            private readonly ImageSource sortGlyph;

            public SortGlyphAdorner(
                GridViewColumnHeader columnHeader,
                ListSortDirection direction,
                ImageSource sortGlyph)
                : base(columnHeader)
            {
                this.columnHeader = columnHeader;
                this.direction = direction;
                this.sortGlyph = sortGlyph;
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);

                if (sortGlyph != null)
                {
                    double x = columnHeader.ActualWidth - 13;
                    double y = (columnHeader.ActualHeight / 2) - 5;
                    Rect rect = new Rect(x, y, 10, 10);
                    drawingContext.DrawImage(sortGlyph, rect);
                }
                else
                {
                    drawingContext.DrawGeometry(Brushes.LightGray, new Pen(Brushes.Gray, 1.0), GetDefaultGlyph());
                }
            }

            private Geometry GetDefaultGlyph()
            {
                var x1 = columnHeader.ActualWidth - 13;
                var x2 = x1 + 10;
                var x3 = x1 + 5;
                var y1 = (columnHeader.ActualHeight / 2) - 3;
                var y2 = y1 + 5;

                if (direction == ListSortDirection.Ascending)
                {
                    var tmp = y1;
                    y1 = y2;
                    y2 = tmp;
                }

                var pathSegmentCollection = new PathSegmentCollection
                                                {
                                                    new LineSegment(new Point(x2, y1), true),
                                                    new LineSegment(new Point(x3, y2), true),
                                                };

                var pathFigure = new PathFigure(new Point(x1, y1), pathSegmentCollection, true);

                var pathFigureCollection = new PathFigureCollection { pathFigure };

                var pathGeometry = new PathGeometry(pathFigureCollection);
                return pathGeometry;
            }
        }
    }
}