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

    using Sentinel.Highlighters;
    using Sentinel.Highlighters.Interfaces;
    using Sentinel.Interfaces;
    using Sentinel.Services;
    using Sentinel.Support;
    using Sentinel.Support.Wpf;

    /// <summary>
    /// Interaction logic for LogMessagesControl.xaml
    /// </summary>
    public partial class LogMessagesControl : UserControl
    {
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
            if (searchHighlighter != null
                && searchHighlighter.Highlighter != null 
                && searchHighlighter.Highlighter is INotifyPropertyChanged )
            {
                (searchHighlighter.Highlighter as INotifyPropertyChanged).PropertyChanged += (s, e) => UpdateStyles();
            }

            messages.ItemContainerStyleSelector = new HighlightingSelector();

            Preferences = ServiceLocator.Instance.Get<IUserPreferences>();
            if (Preferences != null && Preferences is INotifyPropertyChanged)
            {
                (Preferences as INotifyPropertyChanged).PropertyChanged
                    += PreferencesChanged;
            }

            // Read defaulted values from preferences
            UpdateStyles();
            SetDateFormat(Preferences != null ? Preferences.SelectedDateOption : 1);
            SetTypeColumnPreferences(Preferences != null ? Preferences.SelectedTypeOption : 1);
        }

        private void AddCopyCommandBinding()
        {
            ExecutedRoutedEventHandler handler = (s, a) => { CopySelectedLogEntries(); };

            var command = new RoutedCommand("Copy", typeof(GridView));
            command.InputGestures.Add(new KeyGesture(Key.C, ModifierKeys.Control, "Copy"));
            messages.CommandBindings.Add(new CommandBinding(command, handler));

            try
            {
                Clipboard.SetData(DataFormats.Text, string.Empty);
            }
            catch (COMException)
            {
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
                        string.Format(
                            "{0}|{1}|{2}|{3}",
                            item.DateTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.ffff"),
                            item.Type,
                            item.System,
                            item.Description));
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

        private IHighlightingService<IHighlighter> Highlight { get; set; }

        private IUserPreferences Preferences { get; set; }

        public void ScrollToEnd()
        {
            ScrollingHelper.ScrollToEnd(Dispatcher, messages);
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
                SetDateFormat(Preferences.SelectedDateOption);
            }
        }

        private void SetTypeColumnPreferences(int selectedTypeOption)
        {
            if (messages != null)
            {
                // TODO: to cope with resorting of columns, this code should search for the column, not assume it is the first.
                // Get the first column in logDetails and check it is a fixed-width column.
                var view = messages.View as GridView;
                if (view != null && view.Columns[0] is FixedWidthColumn)
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
                        default:
                            break;
                    }
                }
            }
        }

        private void SetDateFormat(int selectedDateOption)
        {
            if (messages != null)
            {
                var view = messages.View as GridView;
                if (view != null)
                {
                    // TODO: to cope with resorting of columns, this code should search for the column, not assume it is the second.
                    var column = view.Columns[1];

                    var dateFormat = "r";
                    switch (selectedDateOption)
                    {
                        case 0:
                            dateFormat = "r";
                            column.Width = 175;
                            break;
                        case 1:
                            dateFormat = "dd/MM/yyyy HH:mm:ss";
                            column.Width = 120;
                            break;
                        case 2:
                            dateFormat = "dddd, d MMM yyyy, HH:mm:ss";
                            column.Width = 170;
                            break;
                        case 3:
                            dateFormat = "HH:mm:ss";
                            column.Width = 60;
                            break;
                        case 4:
                            dateFormat = "HH:mm:ss,fff";
                            column.Width = 80;
                            break;
                        default:
                            break;
                    }

                    column.DisplayMemberBinding = new Binding("DateTime") { StringFormat = dateFormat };
                }
            }
        }

        private void UpdateStyles()
        {
            messages.ItemContainerStyleSelector = null;
            messages.ItemContainerStyleSelector = new HighlightingSelector();
        }
    }
}