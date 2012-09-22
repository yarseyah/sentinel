namespace Sentinel.FileMonitor
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Controls;

    using WpfExtras;

    /// <summary>
    /// Interaction logic for CustomMessageDecoderPage.xaml
    /// </summary>
    public partial class CustomMessageDecoderPage : IWizardPage, IDataErrorInfo
    {
        private readonly ObservableCollection<IWizardPage> children = new ObservableCollection<IWizardPage>();

        private readonly ReadOnlyObservableCollection<IWizardPage> readonlyChildren;

        private string customFormat;

        private string error;

        private bool isValid;

        public CustomMessageDecoderPage()
        {
            InitializeComponent();
            DataContext = this;

            readonlyChildren = new ReadOnlyObservableCollection<IWizardPage>(children);

            PropertyChanged += PropertyChangedHandler;
        }

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if ( e.PropertyName == "CustomFormat")
            {
                IsValid = this["CustomFormat"] == null;
            }
        }

        public string CustomFormat
        {
            get
            {
                return customFormat;
            }
            set
            {
                if (customFormat == value) return;
                customFormat = value;
                OnPropertyChanged("CustomFormat");
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
                return "Custom Message Decoder";
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
                return "Specify how to decompose the message into its individual fields.";
            }
        }

        public bool IsValid
        {
            get
            {
                return isValid;
            }
            set
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
            // Todo: Implement page save....
            return saveData;

            //Debug.Assert(settings != null,
            //             "Settings not set, did the previous page not provide this?  " +
            //             "Was SuggestPreviousPage not called by the caller of this class?");
            //settings.MessageDecoder = CustomFormat;
            //return settings;
        }

        #endregion

        #region Implementation of IDataErrorInfo

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <returns>
        /// The error message for the property. The default is a null.</returns>
        /// <param name="columnName">The name of the property whose error message to get.</param>
        public string this[string columnName]
        {
            get
            {
                if (columnName == "CustomFormat")
                {
                    string err = null;

                    if ( String.IsNullOrEmpty(CustomFormat) )
                    {
                        err = "Pattern can not be empty";
                    }
                    else
                    {
                        // See whether the string validates as a Regex
                        try
                        {
                            Regex r = new Regex(CustomFormat);

                            // See if it contains the minimal fields
                            if ( !ContainsKeyFields(CustomFormat))
                            {
                                err = "The pattern does not define any of the core fields, Description, Type or " +
                                      "DateTime.  At least one of these should be defined.";
                            }
                        }
                        catch (ArgumentException)
                        {
                            err = "The custom pattern does not equate to a valid regular expression";
                        }
                    }

                    Error = err;
                    return err;
                }

                return null;
            }
        }

        private static bool ContainsKeyFields(string pattern)
        {
            string p = pattern.ToLower();

            if (p.Contains("(?<description>") ||
                p.Contains("(?<type>") ||
                p.Contains("(?<datetime>"))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets an error message indicating what is wrong with this object.
        /// </summary>
        /// <returns>
        /// An error message indicating what is wrong with this object. The default is a null.</returns>
        public string Error
        {
            get
            {
                return error;
            }
            private set
            {
                if (error == value) return;
                error = value;
                OnPropertyChanged("Error");
            }
        }

        #endregion

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Set the custom format after the constructor to force the validation
            // to show immediately, this will retrigger the validation.
            OnPropertyChanged("CustomFormat");
        }
    }
}
