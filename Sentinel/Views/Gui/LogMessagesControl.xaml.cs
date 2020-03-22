namespace Sentinel.Views.Gui
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;

    using Common.Logging;

    using Sentinel.Highlighters;
    using Sentinel.Highlighters.Interfaces;
    using Sentinel.Interfaces;
    using Sentinel.Interfaces.CodeContracts;

    using Sentinel.Services;
    using Sentinel.Support;
    using Sentinel.Support.Wpf;

    /// <summary>
    /// Interaction logic for LogMessagesControl.xaml.
    /// </summary>
    public partial class LogMessagesControl : UserControl
    {
        private static readonly ILog Log = LogManager.GetLogger<LogMessagesControl>();

        public LogMessagesControl()
        {
            InitializeComponent();

            AddCopyCommandBinding();

            Highlight = ServiceLocator.Instance.Get<IHighlightingService<IHighlighter>>();

            if (Highlight is INotifyPropertyChanged)
            {
                (Highlight as INotifyPropertyChanged).PropertyChanged += (s, e) => UpdateStyles();
            }

            var searchHighlighter = ServiceLocator.Instance.Get<ISearchHighlighter>();
            if (searchHighlighter?.Highlighter is INotifyPropertyChanged)
            {
                ((INotifyPropertyChanged)searchHighlighter.Highlighter).PropertyChanged += (s, e) => UpdateStyles();
            }

            messages.ItemContainerStyleSelector = new HighlightingSelector(Messages_OnMouseDoubleClick);

            Preferences = ServiceLocator.Instance.Get<IUserPreferences>();
            if (Preferences is INotifyPropertyChanged preferenceChanged)
            {
                preferenceChanged.PropertyChanged += PreferencesChanged;
            }

            // Read defaulted values from preferences
            UpdateStyles();

            UpdateDateFormat();
            SetTypeColumnPreferences(Preferences?.SelectedTypeOption ?? 1);
            DoubleClickToShowExceptions = Preferences != null && Preferences.DoubleClickToShowExceptions;
        }

        ~LogMessagesControl()
        {
            // Unregister observer of Preferences changing.
            if (Preferences is INotifyPropertyChanged preferences)
            {
                preferences.PropertyChanged -= PreferencesChanged;
            }
        }

        private IHighlightingService<IHighlighter> Highlight { get; }

        private IUserPreferences Preferences { get; }

        private bool DoubleClickToShowExceptions { get; set; }

        public void ScrollToEnd()
        {
            ScrollingHelper.ScrollToEnd(Dispatcher, messages);
        }

        private void SetTypeColumnPreferences(int selectedTypeOption)
        {
            // TODO: to cope with resorting of columns, this code should search for the column, not assume it is the first.
            // Get the first column in logDetails and check it is a fixed-width column.
            var view = messages?.View as GridView;

            if (view?.Columns[0] is FixedWidthColumn)
            {
                var fixedColumn = (FixedWidthColumn)view.Columns[0];
                switch (selectedTypeOption)
                {
                    case 0:
                        fixedColumn.FixedWidth = 0;
                        break;
                    case 1:
                        fixedColumn.FixedWidth = 30;
                        break;
                    case 2:
                        fixedColumn.FixedWidth = 60;
                        break;
                    case 3:
                        fixedColumn.FixedWidth = 90;
                        break;
                }
            }
        }

        private void UpdateDateFormat()
        {
            var view = messages?.View as GridView;

            if (view != null)
            {
                // TODO: to cope with resorting of columns, this code should search for the column, not assume it is the second.
                BindDateColumn(view.Columns[1]);
                BindTimeColumn(view.Columns[2]);

                // TODO: need to invalidate all existing ones!
            }
        }

        private void BindTimeColumn(GridViewColumn column)
        {
            column.ThrowIfNull(nameof(column));

            var converter = (IValueConverter)Resources["TimePreferenceConverter"];
            column.DisplayMemberBinding = new Binding(".") { Converter = converter, ConverterParameter = Preferences };
        }

        private void BindDateColumn(GridViewColumn column)
        {
            column.ThrowIfNull(nameof(column));

            var converter = (IValueConverter)Resources["DatePreferenceConverter"];
            column.DisplayMemberBinding = new Binding(".") { Converter = converter, ConverterParameter = Preferences };
        }

        private void UpdateStyles()
        {
            messages.ItemContainerStyleSelector = null;
            messages.ItemContainerStyleSelector = new HighlightingSelector(Messages_OnMouseDoubleClick);
        }

        private void Messages_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            sender.ThrowIfNull(nameof(sender));

            if (DoubleClickToShowExceptions)
            {
                if (sender is ListViewItem item)
                {
                    Log.Trace("Double click performed on entry");

                    if (item.HasContent)
                    {
                        if (item.Content is ILogEntry entry)
                        {
                            Log.Trace(entry.Type);
                            Log.Trace(entry.Description);
                        }
                    }
                }
            }
        }

        private void CopySelectedLogEntries()
        {
            if (messages.SelectedItems.Count != 0)
            {
                var sb = new StringBuilder();
                foreach (ILogEntry item in messages.SelectedItems)
                {
                    sb.AppendLine(
                        $"{item.DateTime.ToLocalTime():yyyy-MM-dd HH:mm:ss.ffff}|{item.Type}|{item.System}|{item.Description}");
                }

                try
                {
                    Clipboard.SetData(DataFormats.Text, sb.ToString());
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Sentinel could not copy to the clipboard", ex);
                }
            }
        }

        private void AddCopyCommandBinding()
        {
            void Handler(object s, ExecutedRoutedEventArgs a)
            {
                CopySelectedLogEntries();
            }

            var command = new RoutedCommand("Copy", typeof(GridView));
            command.InputGestures.Add(new KeyGesture(Key.C, ModifierKeys.Control, "Copy"));
            messages.CommandBindings.Add(new CommandBinding(command, Handler));

            try
            {
                Clipboard.SetData(DataFormats.Text, string.Empty);
            }
            catch (COMException)
            {
            }
        }

        private void PreferencesChanged(object s, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "UseTighterRows")
            {
                UpdateStyles();
            }
            else if (e.PropertyName == "SelectedTypeOption")
            {
                SetTypeColumnPreferences(Preferences.SelectedTypeOption);
            }
            else if (e.PropertyName == "SelectedDateOption")
            {
                UpdateDateFormat();
            }
            else if (e.PropertyName == "DoubleClickToShowExceptions")
            {
                DoubleClickToShowExceptions = Preferences.DoubleClickToShowExceptions;
            }
        }
    }
}