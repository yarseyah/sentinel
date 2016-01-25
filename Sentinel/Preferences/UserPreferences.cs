namespace Sentinel.Preferences
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    using Interfaces;
    using Support.Wpf;

    using WpfExtras;

    /// <summary>
    /// An implementation of the IUserPreferences which holds all of the user
    /// selections in a view-model based structure, allowing simple binding to
    /// the contents for GUIs whilst also allowing other interested parties to
    /// register to elements to be notified when they change.
    /// </summary>
    [DataContract]
    public class UserPreferences : ViewModelBase, IUserPreferences
    {
        private int selectedDateOption;

        private int selectedTimeFormatOption;

        private bool useArrivalDateTime;

        private bool convertUtcTimesToLocalTimeZone = true;

        private int selectedTypeOption = 1;

        private bool show;

        private bool showThreadColumn;

        private bool showExceptionColumn;

        private bool showSourceColumn;

        private bool useLazyRebuild;

        private bool useStackedLayout = true;

        private bool useTighterRows = true;

        private bool doubleClickToShowExceptions = true;

        private bool showSourceInformationColumns;

        /// <summary>
        /// Gets the name of the current Windows theme.
        /// </summary>
        public string CurrentThemeName => ThemeInfo.CurrentThemeFileName;

        /// <summary>
        /// Gets an enumerable list of the available date formatting options.
        /// </summary>
        public IEnumerable<string> DateFormatOptions { get; } = new[]
                                                                    {
                                                                        "yyyy-MM-dd", "MMM-dd", "dd-MM-yyyy", "dd-MMM",
                                                                        "MM-dd-yyyy", "dddd"
                                                                    };

        public IEnumerable<string> TimeFormatOptions { get; } = new[] { "HH:mm:ss;FFFF", "HH:mm:ss", "HH:mm" };

        /// <summary>
        /// Gets or sets the selected date option, as a index of the available options.
        /// </summary>
        /// <see cref="DateFormatOptions"/>
        public int SelectedDateOption
        {
            get
            {
                return selectedDateOption;
            }

            set
            {
                if (selectedDateOption != value)
                {
                    selectedDateOption = value;
                    OnPropertyChanged(nameof(SelectedDateOption));
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected time format, as a index of the available options.
        /// </summary>
        /// <see cref="DateFormatOptions"/>
        public int SelectedTimeFormatOption
        {
            get
            {
                return selectedTimeFormatOption;
            }

            set
            {
                if (selectedTimeFormatOption != value)
                {
                    selectedTimeFormatOption = value;
                    OnPropertyChanged(nameof(SelectedTimeFormatOption));
                }
            }
        }

        public bool ConvertUtcTimesToLocalTimeZone
        {
            get
            {
                return convertUtcTimesToLocalTimeZone;
            }

            set
            {
                if (value != convertUtcTimesToLocalTimeZone)
                {
                    convertUtcTimesToLocalTimeZone = value;
                    OnPropertyChanged(nameof(ConvertUtcTimesToLocalTimeZone));
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the display should be of the parsed date/time or of the one made upon message receipt.
        /// </summary>
        public bool UseArrivalDateTime
        {
            get
            {
                return useArrivalDateTime;
            }

            set
            {
                if (useArrivalDateTime != value)
                {
                    useArrivalDateTime = value;
                    OnPropertyChanged(nameof(UseArrivalDateTime));
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected type option, as a index of the available options.
        /// </summary>
        /// <seealso cref="TypeOptions"/>
        public int SelectedTypeOption
        {
            get
            {
                return selectedTypeOption;
            }

            set
            {
                if (selectedTypeOption != value)
                {
                    selectedTypeOption = value;
                    OnPropertyChanged(nameof(SelectedTypeOption));
                }
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
                if (show != value)
                {
                    show = value;
                    OnPropertyChanged("Show");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the thread column should be shown or not.
        /// </summary>
        public bool ShowThreadColumn
        {
            get
            {
                return showThreadColumn;
            }

            set
            {
                if (showThreadColumn != value)
                {
                    showThreadColumn = value;
                    OnPropertyChanged("ShowThreadColumn");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the thread column should be shown or not.
        /// </summary>
        public bool ShowSourceColumn
        {
            get
            {
                return showSourceColumn;
            }

            set
            {
                if (showSourceColumn != value)
                {
                    showSourceColumn = value;
                    OnPropertyChanged(nameof(ShowSourceColumn));
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the exception column should be shown or not.
        /// </summary>
        public bool ShowExceptionColumn
        {
            get
            {
                return showExceptionColumn;
            }

            set
            {
                if (showExceptionColumn != value)
                {
                    showExceptionColumn = value;
                    OnPropertyChanged("ShowExceptionColumn");
                }
            }
        }

        /// <summary>
        /// Gets a list of the available type column options, such as hidden, icons, text, etc.
        /// </summary>
        public IEnumerable<string> TypeOptions => new[] { "Hidden", "Icons", "Text", "Icon and text" };

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
        public bool UseLazyRebuild
        {
            get
            {
                return useLazyRebuild;
            }

            set
            {
                if (useLazyRebuild != value)
                {
                    useLazyRebuild = value;
                    OnPropertyChanged("UseLazyRebuild");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the log messages and activity windows should be
        /// displayed with on top of each other (stacked) or beside each other.
        /// </summary>
        public bool UseStackedLayout
        {
            get
            {
                return useStackedLayout;
            }

            set
            {
                if (useStackedLayout != value)
                {
                    useStackedLayout = value;
                    OnPropertyChanged("UseStackedLayout");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a visual padding correction should be used to
        /// tighten the rows in a list view.  Windows Vista and Windows 7 both use much more padding
        /// around each row than Windows XP does.  Sometimes the XP look works better!
        /// </summary>
        public bool UseTighterRows
        {
            get
            {
                return useTighterRows;
            }

            set
            {
                if (useTighterRows != value)
                {
                    useTighterRows = value;
                    OnPropertyChanged("UseTighterRows");
                }
            }
        }

        public bool DoubleClickToShowExceptions
        {
            get
            {
                return doubleClickToShowExceptions;
            }

            set
            {
                if (doubleClickToShowExceptions != value)
                {
                    doubleClickToShowExceptions = value;
                    OnPropertyChanged("DoubleClickToShowExceptions");
                }
            }
        }

        public bool ShowSourceInformationColumns
        {
            get
            {
                return showSourceInformationColumns;
            }

            set
            {
                if (showSourceInformationColumns != value)
                {
                    showSourceInformationColumns = value;
                    OnPropertyChanged("ShowSourceInformationColumns");
                }
            }
        }
    }
}