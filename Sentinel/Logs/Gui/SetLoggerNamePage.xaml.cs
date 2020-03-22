namespace Sentinel.Logs.Gui
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows.Controls;

    using Sentinel.Logs.Interfaces;
    using WpfExtras;

    /// <summary>
    /// Interaction logic for SetLoggerNamePage.xaml.
    /// </summary>
    public partial class SetLoggerNamePage : IWizardPage, IDataErrorInfo
    {
        private readonly ObservableCollection<IWizardPage> children = new ObservableCollection<IWizardPage>();

        private readonly ILogManager logManager = Services.ServiceLocator.Instance.Get<ILogManager>();

        private string logName = "Untitled";

        private bool isValid;

        public SetLoggerNamePage()
        {
            InitializeComponent();
            DataContext = this;
            Children = new ReadOnlyObservableCollection<IWizardPage>(children);
            PropertyChanged += PropertyChangedHandler;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string LogName
        {
            get
            {
                return logName;
            }

            set
            {
                if (logName != value)
                {
                    logName = value;
                    OnPropertyChanged(nameof(LogName));
                }
            }
        }

        public string Title => "Log Name";

        public ReadOnlyObservableCollection<IWizardPage> Children { get; }

        public string Description => "Define a name for the log to be created.";

        public bool IsValid
        {
            get
            {
                return isValid;
            }

            private set
            {
                if (isValid != value)
                {
                    isValid = value;
                    OnPropertyChanged(nameof(IsValid));
                }
            }
        }

        public Control PageContent => this;

        /// <summary>
        /// Gets an error message indicating what is wrong with this object.
        /// </summary>
        /// <returns>
        /// An error message indicating what is wrong with this object. The default is an empty string ("").
        /// </returns>
        public string Error => this["LogName"];

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <returns>
        /// The error message for the property.
        /// </returns>
        /// <param name="columnName">The name of the property whose error message to get.</param>
        public string this[string columnName]
        {
            get
            {
                if (columnName == "LogName")
                {
                    if (string.IsNullOrEmpty(LogName))
                    {
                        return "Log name may not be blank.";
                    }

                    if (logManager != null && logManager.Any(l => l.Name == LogName))
                    {
                        return "A logger with that name already exists";
                    }
                }

                return null;
            }
        }

        public object Save(object saveData)
        {
            Debug.Assert(saveData is NewLoggerSettings, "Expecting to have a NewLoggerSettings instance");
            Debug.Assert(saveData as NewLoggerSettings != null, "Not expecting a null");

            var settings = saveData as NewLoggerSettings;
            if (settings != null)
            {
                settings.LogName = LogName;
            }

            return saveData;
        }

        public void AddChild(IWizardPage newItem)
        {
            children.Add(newItem);
            OnPropertyChanged(nameof(Children));
        }

        public void RemoveChild(IWizardPage item)
        {
            children.Remove(item);
            OnPropertyChanged(nameof(Children));
        }

        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LogName")
            {
                // Validate against standard validation rules.
                IsValid = this["LogName"] == null;
            }
        }

        private void PageLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            OnPropertyChanged(nameof(LogName));
        }
    }
}
