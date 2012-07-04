#region Using directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using Sentinel.Providers.Interfaces;
using WpfExtras;
using DelegateCommand = Sentinel.Support.Mvvm.DelegateCommand;

#endregion

namespace Sentinel.Providers
{
    using Sentinel.Interfaces.Providers;

    /// <summary>
    ///   Interaction logic for FileMonitorProviderPage.xaml
    /// </summary>
    public partial class FileMonitorProviderPage : IWizardPage, IDataErrorInfo
    {
        private readonly ObservableCollection<IWizardPage> children = new ObservableCollection<IWizardPage>();

        private readonly ReadOnlyObservableCollection<IWizardPage> readonlyChildren;

        private string fileName;

        private bool loadExisting;
        
        private double refresh;
        
        private bool warnFileNotFound = true;
        
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

        public ICommand Browse
        {
            get;
            private set;
        }

        public string FileName
        {
            get
            {
                return fileName;
            }
            set
            {
                if (fileName == value) return;
                fileName = value;
                OnPropertyChanged("FileName");
            }
        }

        public bool WarnFileNotFound
        {
            get
            {
                return warnFileNotFound;
            }
            set
            {
                if (warnFileNotFound == value) return;
                Trace.WriteLine("Setting WarnFileNotFound to " + value);
                warnFileNotFound = value;
                OnPropertyChanged("WarnFileNotFound");
            }
        }

        public double Refresh
        {
            get
            {
                return refresh;
            }
            set
            {
                if (refresh == value) return;
                refresh = value;
                OnPropertyChanged("Refresh");
            }
        }

        public int MaxRefresh
        {
            get
            {
                return 5000;
            }
        }

        public int MinRefresh
        {
            get
            {
                return 50;
            }
        }


        public bool LoadExisting
        {
            get
            {
                return loadExisting;
            }
            set
            {
                if (loadExisting == value) return;
                loadExisting = value;
                OnPropertyChanged("LoadExisting");
            }
        }

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion

        #region Implementation of IWizardPage

        public string Title
        {
            get
            {
                return "Log file monitoring provider";
            }
        }

        public ReadOnlyObservableCollection<IWizardPage> Children
        {
            get
            {
                return readonlyChildren;
            }
        }

        public string Description
        {
            get
            {
                return "Configure Sentinel to monitor a file for new entries";
            }
        }

        public bool IsValid
        {
            get
            {
                return isValid;
            }
            private set
            {
                if (isValid == value) return;
                isValid = value;
                OnPropertyChanged("IsValid");
            }
        }

        public Control PageContent
        {
            get
            {
                return this;
            }
        }

        public void AddChild(IWizardPage newItem)
        {
            children.Add(newItem);
            OnPropertyChanged("Children");
        }

        public void RemoveChild(IWizardPage item)
        {
            children.Remove(item);
            OnPropertyChanged("Children");
        }

        public object Save(object saveData)
        {
            Debug.Assert(saveData != null, "Expecting a non-null instance of a class to save settings into");
            Debug.Assert(saveData is IProviderSettings, "Expecting save object to be an IProviderSettings");

            IProviderSettings settings = saveData as IProviderSettings;

            if ( settings != null )
            {
                IFileMonitoringProviderSettings fileSettings = settings as IFileMonitoringProviderSettings;

                if ( fileSettings != null )
                {
                    fileSettings.Update(fileName, (int) Refresh, LoadExisting);
                    return fileSettings;
                }

                return new FileMonitoringProviderSettings(
                    settings.Info,
                    settings.Name,
                    fileName,
                    (int) Refresh,
                    LoadExisting);
            }

            return saveData;
        }

        #endregion

        #region Implementation of IDataErrorInfo

        /// <summary>
        ///   Gets the error message for the property with the given name.
        /// </summary>
        /// <returns>
        ///   The error message for the property. The default is an empty string ("").
        /// </returns>
        /// <param name = "columnName">The name of the property whose error message to get.</param>
        public string this[string columnName]
        {
            get
            {
                if (columnName == "FileName")
                {
                    if (String.IsNullOrEmpty(FileName))
                    {
                        return "File name not specified";
                    }

                    try
                    {
                        FileInfo fi = new FileInfo(FileName);
                    }
                    catch (NotSupportedException)
                    {
                        return "The file name specified is not valid for a file.";
                    }
                    catch (ArgumentException)
                    {
                        return "The file name specified is not valid for a file.";
                    }
                    catch (PathTooLongException)
                    {
                        return "The file name specified is too long to be a valid file.";
                    }
                    catch (SecurityException)
                    {
                        return "You do not have permission to work with that file/location.";
                    }
                }

                return null;
            }
        }

        /// <summary>
        ///   Gets an error message indicating what is wrong with this object.
        /// </summary>
        /// <returns>
        ///   An error message indicating what is wrong with this object. The default is an empty string ("").
        /// </returns>
        public string Error
        {
            get
            {
                return this["FileName"];
            }
        }

        #endregion

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "FileName")
            {
                try
                {
                    FileInfo fi = new FileInfo(FileName);
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
        }

        private void BrowseForFile(object obj)
        {
            // Display the File Open Dialog.
            OpenFileDialog dlg =
                new OpenFileDialog
                    {
                        FileName = "logFile",
                        DefaultExt = ".log",
                        Multiselect = false,
                        Filter =
                            "Log files (.log)|*.log"
                            + "|Text documents (.txt)|*.txt"
                            + "|All files|*.*"
                    };

            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                FileName = dlg.FileName;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            FileName = string.Empty;
            Refresh = 250;
        }
    }
}