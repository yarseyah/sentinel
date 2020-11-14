namespace Sentinel.FileMonitor
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Security;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using Microsoft.Win32;

    using Sentinel.Interfaces.Providers;

    using WpfExtras;

    /// <summary>
    ///   Interaction logic for FileMonitorProviderPage.xaml.
    /// </summary>
    public partial class FileMonitorProviderPage : IWizardPage, IDataErrorInfo
    {
        private readonly ObservableCollection<IWizardPage> children = new ObservableCollection<IWizardPage>();

        private readonly ReadOnlyObservableCollection<IWizardPage> readonlyChildren;

        private string fileName = string.Empty;

        private bool loadExisting;

        private double refresh;

        private bool warnFileNotFound;

        private bool isValid;

        public FileMonitorProviderPage()
        {
            InitializeComponent();

            DataContext = this;
            readonlyChildren = new ReadOnlyObservableCollection<IWizardPage>(children);

            PropertyChanged += PropertyChangedHandler;

            Browse = new DelegateCommand(BrowseForFile);

            // Need a subsequent page to define message format.
            AddChild(new MessageFormatPage());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand Browse { get; private set; }

        public string FileName
        {
            get => fileName;

            set
            {
                if (fileName != value)
                {
                    fileName = value;
                    OnPropertyChanged(nameof(FileName));
                }
            }
        }

        public bool WarnFileNotFound
        {
            get => warnFileNotFound;

            set
            {
                if (warnFileNotFound != value)
                {
                    Trace.WriteLine("Setting WarnFileNotFound to " + value);
                    warnFileNotFound = value;
                    OnPropertyChanged(nameof(WarnFileNotFound));
                }
            }
        }

        public double Refresh
        {
            get => refresh;

            set
            {
                if (Math.Abs(refresh - value) > 0.01)
                {
                    refresh = value;
                    OnPropertyChanged(nameof(Refresh));
                }
            }
        }

        public int MaxRefresh => 5000;

        public int MinRefresh => 50;

        public bool LoadExisting
        {
            get => loadExisting;

            set
            {
                if (loadExisting != value)
                {
                    loadExisting = value;
                    OnPropertyChanged(nameof(LoadExisting));
                }
            }
        }

        public string Title => "Log file monitoring provider";

        public ReadOnlyObservableCollection<IWizardPage> Children => readonlyChildren;

        public Control PageContent => this;

        public string Description => "Configure Sentinel to monitor a file for new entries";

        public bool IsValid
        {
            get => isValid;

            private set
            {
                if (isValid != value)
                {
                    isValid = value;
                    OnPropertyChanged(nameof(IsValid));
                }
            }
        }

        /// <summary>
        ///   Gets an error message indicating what is wrong with this object.
        /// </summary>
        /// <returns>
        ///   An error message indicating what is wrong with this object. The default is an empty string ("").
        /// </returns>
        public string Error => this["FileName"];

        /// <summary>
        ///   Gets the error message for the property with the given name.
        /// </summary>
        /// <param name = "columnName">The name of the property whose error message to get.</param>
        /// <returns>The error message for the property. The default is an empty string ("").</returns>
        public string this[string columnName]
        {
            get
            {
                if (columnName != "FileName")
                {
                    return null;
                }

                if (string.IsNullOrWhiteSpace(FileName))
                {
                    return "File name not specified";
                }

                string reason;
                return !CheckSuppliedFilenameIsValid(FileName, out reason) ? reason : null;
            }
        }

        public object Save(object saveData)
        {
            Debug.Assert(saveData != null, "Expecting a non-null instance of a class to save settings into");
            Debug.Assert(saveData is IProviderSettings, "Expecting save object to be an IProviderSettings");

            var settings = saveData as IProviderSettings;

            if (settings != null)
            {
                if (settings is IFileMonitoringProviderSettings fileSettings)
                {
                    fileSettings.Update(fileName, (int)Refresh, LoadExisting);
                    return fileSettings;
                }

                return new FileMonitoringProviderSettings(
                    settings.Info,
                    settings.Name,
                    fileName,
                    (int)Refresh,
                    LoadExisting);
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

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        private void BrowseForFile(object obj)
        {
            // Display the File Open Dialog.
            var dlg = new OpenFileDialog
                          {
                              FileName = "logFile",
                              DefaultExt = ".log",
                              Multiselect = false,
                              Filter = "Log files (.log)|*.log|Text documents (.txt)|*.txt|All files|*.*",
                          };

            var result = dlg.ShowDialog();

            if (result == true)
            {
                FileName = dlg.FileName;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Refresh = 250;
        }

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "FileName")
            {
                return;
            }

            try
            {
                var fi = new FileInfo(FileName);
                WarnFileNotFound = !fi.Exists;
                IsValid = this["FileName"] == null;
            }
            catch (Exception)
            {
                // For exceptions, let the validation handler show the error.
                WarnFileNotFound = false;
                IsValid = false;
            }
        }

        private bool CheckSuppliedFilenameIsValid(string fileNameToValidate, out string reason)
        {
            try
            {
                reason = null;
                _ = new FileInfo(fileNameToValidate);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                reason = "The file name specified is in a location unauthorised";
            }
            catch (NotSupportedException)
            {
                reason = "The file name specified is not valid for a file.";
            }
            catch (ArgumentException)
            {
                reason = "The file name specified is not valid for a file.";
            }
            catch (PathTooLongException)
            {
                reason = "The file name specified is too long to be a valid file.";
            }
            catch (SecurityException)
            {
                reason = "You do not have permission to work with that file/location.";
            }

            return false;
        }
    }
}