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
using ProtoBuf;
using Sentinel.Interfaces;
using Sentinel.Support.Mvvm;
using Sentinel.Support.Wpf;

#endregion

namespace Sentinel.Preferences
{
    /// <summary>
    /// An implementation of the IUserPreferences which holds all of the user
    /// selections in a view-model based structure, allowing simple binding to 
    /// the contents for GUIs whilst also allowing other interested parties to
    /// register to elements to be notified when they change.
    /// </summary>
    [ProtoContract]
    public class UserPreferences 
        : ViewModelBase
        , IUserPreferences
    {
        private readonly IList<string> dateFormatOptions = new List<string>
                                                               {
                                                                   "Default",
                                                                   "Short Date",
                                                                   "Long Date",
                                                                   "Time",
                                                                   "Time with Milliseconds"
                                                               };

        private readonly IList<string> typeColumnOptions = new List<string>
                                                               {
                                                                   "Hidden",
                                                                   "Icons",
                                                                   "Text",
                                                                   "Icon and text"
                                                               };

        private int selectedDateOption;

        private int selectedTypeOption = 1;

        private bool show;

        private bool showThreadColumn;

        private bool useLazyRebuild;

        private bool useStackedLayout = true;

        private bool useTighterRows;

        #region IUserPreferences Members

        /// <summary>
        /// Gets the name of the current Windows theme.
        /// </summary>
        public string CurrentThemeName
        {
            get
            {
                return ThemeInfo.CurrentThemeFileName;
            }
        }

        /// <summary>
        /// Gets an enumerable list of the available date formatting options.
        /// </summary>
        public IEnumerable<string> DateFormatOptions
        {
            get
            {
                return dateFormatOptions;
            }
        }

        /// <summary>
        /// Gets or sets the selected date option, as a index of the available options.
        /// </summary>
        /// <see cref="DateFormatOptions"/>
        /// <remarks>Assigned member number 1 in protobuf's serialization model</remarks>
        [ProtoMember(1, IsRequired = true)]
        public int SelectedDateOption
        {
            get
            {
                return selectedDateOption;
            }

            set
            {
                if (selectedDateOption == value) return;
                selectedDateOption = value;
                OnPropertyChanged("SelectedDateOption");
            }
        }

        /// <summary>
        /// Gets or sets the selected type option, as a index of the available options.
        /// </summary>
        /// <seealso cref="TypeOptions"/>
        [ProtoMember(2, IsRequired = true)]
        public int SelectedTypeOption
        {
            get
            {
                return selectedTypeOption;
            }

            set
            {
                if (value == selectedTypeOption) return;
                selectedTypeOption = value;
                OnPropertyChanged("SelectedTypeOption");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user preferences should be shown.
        /// </summary>
        public bool Show
        {
            get
            {
                return show;
            }

            set
            {
                if (value == show) return;
                show = value;
                OnPropertyChanged("Show");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the thread column should be shown or not.
        /// </summary>
        [ProtoMember(3, IsRequired = true)]
        public bool ShowThreadColumn
        {
            get
            {
                return showThreadColumn;
            }

            set
            {
                if (value == showThreadColumn) return;
                showThreadColumn = value;
                OnPropertyChanged("ShowThreadColumn");
            }
        }

        /// <summary>
        /// Gets a list of the available type column options, such as hidden, icons, text, etc.
        /// </summary>
        public IEnumerable<string> TypeOptions
        {
            get
            {
                return typeColumnOptions;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the lazy rebuilding option should be used
        /// for rebuilding sorted views.
        /// </summary>
        /// <remarks>
        /// Lazy in this case means throwing away the existing collection and building
        /// a new one.  This isn't optimal in terms of memory nor functionality, things such as
        /// selected index in a data bound ListView can't be maintained for long.
        /// Visually, this works around an issue, but at the expense performance, memory, etc.
        /// </remarks>
        [ProtoMember(4, IsRequired = true)]
        public bool UseLazyRebuild
        {
            get
            {
                return useLazyRebuild;
            }

            set
            {
                if (value == useLazyRebuild) return;
                useLazyRebuild = value;
                OnPropertyChanged("UseLazyRebuild");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the log messages and activity windows should be
        /// displayed with on top of each other (stacked) or beside each other.
        /// </summary>
        [ProtoMember(5, IsRequired = true)]
        public bool UseStackedLayout
        {
            get
            {
                return useStackedLayout;
            }

            set
            {
                if (value == useStackedLayout) return;
                useStackedLayout = value;
                OnPropertyChanged("UseStackedLayout");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a visual padding correction should be used to 
        /// tighten the rows in a list view.  Windows Vista and Windows 7 both use much more padding
        /// around each row than Windows XP does.  Sometimes the XP look works better!
        /// </summary>
        [ProtoMember(6, IsRequired = true)]
        public bool UseTighterRows
        {
            get
            {
                return useTighterRows;
            }

            set
            {
                if (value == useTighterRows) return;
                useTighterRows = value;
                OnPropertyChanged("UseTighterRows");
            }
        }

        #endregion
    }
}