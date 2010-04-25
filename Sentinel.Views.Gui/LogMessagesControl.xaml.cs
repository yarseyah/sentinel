#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

using System.ComponentModel;
using System.Windows.Controls;
using Sentinel.Highlighters;
using Sentinel.Highlighters.Interfaces;
using Sentinel.Interfaces;
using Sentinel.Logs.Interfaces;
using Sentinel.Services;
using Sentinel.Support;

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

            Highlight = ServiceLocator.Instance.Get<IHighlightingService>();
            if (Highlight != null && Highlight is INotifyPropertyChanged)
            {
                (Highlight as INotifyPropertyChanged).PropertyChanged += (s, e) => UpdateStyles();
            }

            IQuickHighlighter quickHighlighter = ServiceLocator.Instance.Get<IQuickHighlighter>();
            if (quickHighlighter != null
                && quickHighlighter.Highlighter != null 
                && quickHighlighter.Highlighter is INotifyPropertyChanged )
            {
                (quickHighlighter.Highlighter as INotifyPropertyChanged).PropertyChanged += (s, e) => UpdateStyles();
            }

            messages.ItemContainerStyleSelector = new HighlightingSelector();

            Preferences = ServiceLocator.Instance.Get<IUserPreferences>();
            if (Preferences != null && Preferences is INotifyPropertyChanged)
            {
                (Preferences as INotifyPropertyChanged).PropertyChanged
                    += (s, e) =>
                           {
                               if (e.PropertyName == "UseTighterRows")
                               {
                                   UpdateStyles();
                               }
                           };
            }
        }

        private IHighlightingService Highlight { get; set; }

        private IUserPreferences Preferences { get; set; }

        private void UpdateStyles()
        {
            messages.ItemContainerStyleSelector = null;
            messages.ItemContainerStyleSelector = new HighlightingSelector();
        }

        public void ScrollToEnd()
        {
            ScrollingHelper.ScrollToEnd(Dispatcher, messages);
        }
    }
}