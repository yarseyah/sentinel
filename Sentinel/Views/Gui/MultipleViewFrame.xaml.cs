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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sentinel.Filters.Interfaces;
using Sentinel.Highlighters.Interfaces;
using Sentinel.Interfaces;
using Sentinel.Logs.Interfaces;
using Sentinel.Services;
using Sentinel.Support.Wpf;
using Sentinel.Views.Interfaces;

#endregion

namespace Sentinel.Views.Gui
{
    using System.Diagnostics;

    using Sentinel.Filters;

    /// <summary>
    /// Interaction logic for MultipleViewFrame.xaml
    /// </summary>
    public partial class MultipleViewFrame : INotifyPropertyChanged, IWindowFrame
    {
        private readonly IUserPreferences preferences = ServiceLocator.Instance.Get<IUserPreferences>();

        //private readonly ISearchHighlighter searchHighlighter;
        private readonly IViewManager viewManager = ServiceLocator.Instance.Get<IViewManager>();
        private ILogger log;
        private string primaryTitle;
        private ILogViewer primaryView;
        private string secondaryTitle;
        private ILogViewer secondaryView;
        private bool collapseSecondaryView;

        public MultipleViewFrame()
        {
            InitializeComponent();

            if (preferences is INotifyPropertyChanged)
            {
                (preferences as INotifyPropertyChanged).PropertyChanged += PreferencesChanged;
            }

            //Filters = ServiceLocator.Instance.Get<IFilteringService<IFilter>>();
            //searchHighlighter = ServiceLocator.Instance.Get<ISearchHighlighter>();

            SetupSplitter();

            DataContext = this;
        }

        public ICommand Clear { get; private set; }

        public ICommand ClearActivity { get; private set; }

        public ICommand Save { get; private set; }

        //public IFilteringService<IFilter> Filters { get; private set; }
        
        public ILogViewer PrimaryView
        {
            get
            {
                return primaryView;
            }
            set
            {
                if (primaryView == value) return;
                primaryView = value;
                OnPropertyChanged("PrimaryView");
            }
        }


        public string PrimaryTitle
        {
            get
            {
                return primaryTitle;
            }
            set
            {
                if (primaryTitle == value) return;
                primaryTitle = value;
                OnPropertyChanged("PrimaryTitle");
            }
        }


        public string SecondaryTitle
        {
            get
            {
                return secondaryTitle;
            }
            set
            {
                if (secondaryTitle == value) return;
                secondaryTitle = value;
                OnPropertyChanged("SecondaryTitle");
            }
        }

        public ILogViewer SecondaryView
        {
            get
            {
                return secondaryView;
            }
            set
            {
                if (secondaryView == value) return;
                secondaryView = value;
                OnPropertyChanged("SecondaryView");
            }
        }

        public IUserPreferences Preferences
        {
            get
            {
                return preferences;
            }
        }

        //public string Search
        //{
        //    get
        //    {
        //        Debug.Assert(searchHighlighter != null, "Should always have a search highlighter");
        //        return searchHighlighter.Search;
        //    }

        //    set
        //    {
        //        searchHighlighter.Search = value;
        //    }
        //}

        #region IWindowFrame Members

        public ILogger Log
        {
            get
            {
                return log;
            }
            set
            {
                if (log == value) return;
                log = value;
                OnPropertyChanged("Log");
            }
        }

        public void SetViews(IEnumerable<string> viewIdentifiers)
        {
            if (viewIdentifiers != null && viewIdentifiers.Count() >= 1)
            {
                string guid = viewIdentifiers.ElementAt(0);
                PrimaryView = viewManager.GetInstance(guid);
                PrimaryView.SetLogger(log);
                PrimaryTitle = viewManager.Get(guid).Name;
            }
            if (viewIdentifiers != null && viewIdentifiers.Count() >= 2)
            {
                string guid = viewIdentifiers.ElementAt(1);
                SecondaryView = viewManager.GetInstance(guid);
                SecondaryView.SetLogger(log);
                SecondaryTitle = viewManager.Get(guid).Name;
            }

            if ( viewIdentifiers != null && viewIdentifiers.Count()  ==1)
            {
                CollapseSecondaryView();
            }
        }

        #endregion

        ~MultipleViewFrame()
        {
            if (preferences is INotifyPropertyChanged)
            {
                (preferences as INotifyPropertyChanged).PropertyChanged -= PreferencesChanged;
            }
        }

        private void PreferencesChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "UseStackedLayout")
            {
                SetupSplitter();
            }
        }

        private void SetupSplitter()
        {
            if (!collapseSecondaryView)
            {
                bool vertical = preferences.UseStackedLayout;

                int rowSpan = vertical ? 1 : 3;
                int colSpan = vertical ? 3 : 1;

                Grid.SetRowSpan(first, rowSpan);
                Grid.SetRowSpan(splitter, rowSpan);
                Grid.SetRowSpan(second, rowSpan);

                Grid.SetColumnSpan(first, colSpan);
                Grid.SetColumnSpan(splitter, colSpan);
                Grid.SetColumnSpan(second, colSpan);

                splitter.Width = vertical ? Int32.MaxValue : 5;
                splitter.Height = vertical ? 5 : Int32.MaxValue;
                splitter.HorizontalAlignment = vertical
                                                   ? HorizontalAlignment.Stretch
                                                   : HorizontalAlignment.Left;
                splitter.VerticalAlignment = vertical
                                                 ? VerticalAlignment.Top
                                                 : VerticalAlignment.Stretch;

                Grid.SetColumn(first, 0);
                Grid.SetRow(first, 0);

                Grid.SetColumn(splitter, vertical ? 0 : 1);
                Grid.SetRow(splitter, vertical ? 1 : 0);

                Grid.SetColumn(second, vertical ? 0 : 2);
                Grid.SetRow(second, vertical ? 2 : 0);

                splitter.ResizeBehavior = GridResizeBehavior.PreviousAndNext;
                splitter.ResizeDirection = vertical ? GridResizeDirection.Rows : GridResizeDirection.Columns;
            }
            else
            {
                Grid.SetRowSpan(first, 3);
                Grid.SetColumnSpan(first, 3);

                splitter.Visibility = Visibility.Hidden;
                second.Visibility = Visibility.Hidden;
            }
        }

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion

        private void CollapseSecondaryView()
        {
            collapseSecondaryView = true;
            SetupSplitter();
        }

    }
}