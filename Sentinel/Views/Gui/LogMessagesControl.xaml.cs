#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using Sentinel.Highlighters;
using Sentinel.Highlighters.Interfaces;
using Sentinel.Interfaces;
using Sentinel.Logs.Interfaces;
using Sentinel.Services;
using Sentinel.Support;
using Sentinel.Support.Wpf;

#endregion

namespace Sentinel.Views.Gui
{
    /// <summary>
    /// Interaction logic for LogMessagesControl.xaml
    /// </summary>
    public partial class LogMessagesControl : UserControl
    {
        public LogMessagesControl()
        {
            InitializeComponent();

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