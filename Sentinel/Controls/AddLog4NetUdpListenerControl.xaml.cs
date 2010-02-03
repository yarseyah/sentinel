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
using Sentinel.Logger;
using Sentinel.Networking;
using Sentinel.Services;

#endregion

namespace Sentinel.Controls
{

    #region Using directives

    #endregion

    /// <summary>
    /// Interaction logic for AddLog4NetUdpListenerControl.xaml
    /// </summary>
    public partial class AddLog4NetUdpListenerControl
        : IDataErrorInfo, INotifyPropertyChanged, IUdpListenerDetails
    {
        private readonly Dictionary<NameError, string> nameErrors
            = new Dictionary<NameError, string>
                  {
                      {NameError.NotSupplied, "Name field is a required field."},
                      {NameError.Duplicate, "The supplied name has already been used."},
                      {NameError.NoError, null}
                  };

        private readonly Dictionary<PortError, string> portErrors
            = new Dictionary<PortError, string>
                  {
                      {PortError.NotSupplied, "Port number is required"},
                      {PortError.Duplicate, "The specified port number duplicates one used in another listener"},
                      {PortError.NotNumber, "The input supplied is not a valid number"},
                      {PortError.SystemRange, "The specified number lies within the normal system-ports range."},
                      {PortError.NoError, null}
                  };

        private readonly IViewManager views = ServiceLocator.Instance.Get<IViewManager>();

        private bool enabledImmediately = true;

        private string logViewerName = "UDP Listener";

        private NameError nameError = NameError.NotSupplied;

        private string portAsText = "9999";

        private PortError portError = PortError.NotSupplied;

        public AddLog4NetUdpListenerControl()
        {
            InitializeComponent();
            DataContext = this;

            // Lazy update of error messages on each change (except Error!)
            PropertyChanged += (s, e) =>
                                   {
                                       if (e.PropertyName != "Error")
                                       {
                                           OnPropertyChanged("Error");
                                       }
                                   };
        }

        public string PortAsText
        {
            get
            {
                return portAsText;
            }

            set
            {
                if (portAsText != value)
                {
                    portAsText = value;
                    OnPropertyChanged("PortAsText");
                }
            }
        }

        #region IDataErrorInfo Members

        public string Error
        {
            get
            {
                return this["LogViewerName"] ?? this["PortAsText"];
            }
        }

        public string this[string propertyName]
        {
            get
            {
                string errorText = null;

                switch (propertyName)
                {
                    case "PortAsText":
                        portError = ValidatePort();
                        if (portError != PortError.NoError)
                        {
                            errorText = portErrors[portError];
                        }

                        break;
                    case "LogViewerName":
                        nameError = ValidateName();
                        if (nameError != NameError.NoError)
                        {
                            errorText = nameErrors[nameError];
                        }

                        break;
                }

                return errorText;
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region IUdpListenerDetails Members

        public bool Enabled
        {
            get
            {
                return enabledImmediately;
            }

            set
            {
                if (enabledImmediately != value)
                {
                    enabledImmediately = value;
                    OnPropertyChanged("Enabled");
                }
            }
        }

        public string LogViewerName
        {
            get
            {
                return logViewerName;
            }

            set
            {
                if (logViewerName != value)
                {
                    logViewerName = value;
                    OnPropertyChanged("LogViewerName");
                }
            }
        }

        public int Port
        {
            get
            {
                int portAsInt;
                Int32.TryParse(PortAsText, out portAsInt);
                return portAsInt;
            }
        }

        #endregion

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        private NameError ValidateName()
        {
            NameError error = NameError.NoError;

            if (String.IsNullOrEmpty(LogViewerName))
            {
                error = NameError.NotSupplied;
            }
            else if (views != null && views.Viewers.Any(v => v.Name == LogViewerName))
            {
                error = NameError.Duplicate;
            }

            return error;
        }

        private PortError ValidatePort()
        {
            PortError error = PortError.NoError;

            int asInt;

            if (string.IsNullOrEmpty(portAsText))
            {
                error = PortError.NotSupplied;
            }
            else if (!Int32.TryParse(PortAsText, out asInt))
            {
                error = PortError.NotNumber;
            }
            else if (asInt < 1024)
            {
                error = PortError.SystemRange;
            }
            else if (views != null && views.Viewers.OfType<IUdpLogViewer>().Any(v => v.Port == asInt))
            {
                error = PortError.Duplicate;
            }

            return error;
        }

        #region Nested type: NameError

        /// <summary>
        /// Data validation error states for the Name field.
        /// </summary>
        private enum NameError
        {
            /// <summary>
            /// Name not supplied.
            /// </summary>
            NotSupplied,

            /// <summary>
            /// Name is a duplicate, must be unique.
            /// </summary>
            Duplicate,

            /// <summary>
            /// No error condition encountered.
            /// </summary>
            NoError
        }

        #endregion

        #region Nested type: PortError

        /// <summary>
        /// Data validation errors for the port number.
        /// </summary>
        private enum PortError
        {
            /// <summary>
            /// Supplied value is not a number.
            /// </summary>
            NotNumber,

            /// <summary>
            /// Supplied value duplicates another port being used.
            /// </summary>
            Duplicate,

            /// <summary>
            /// Supplied port is in the area typically reserved for system operations
            /// and therefore is not permitted for use.
            /// </summary>
            SystemRange,

            /// <summary>
            /// No error condition encountered.
            /// </summary>
            NoError,

            /// <summary>
            /// Port number has not been supplied.
            /// </summary>
            NotSupplied
        }

        #endregion
    }
}