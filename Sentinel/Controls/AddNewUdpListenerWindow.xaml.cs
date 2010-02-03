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
using System.Windows.Input;
using Sentinel.Networking;
using Sentinel.Support;

#endregion

namespace Sentinel.Controls
{

    #region Using directives

    #endregion

    /// <summary>
    /// Interaction logic for AddNewUdpListenerWindow.xaml
    /// </summary>
    public partial class AddNewUdpListenerWindow : IUdpListenerDetails
    {
        private bool enabled;

        private string logViewerName;

        private int port;

        public AddNewUdpListenerWindow()
        {
            InitializeComponent();
            DataContext = this;

            OK = new DelegateCommand(e => CloseDialog(true), q => IsValid);
            Cancel = new DelegateCommand(e => CloseDialog(false));
        }

        public ICommand Cancel { get; private set; }

        public ICommand OK { get; private set; }

        private bool IsValid
        {
            get
            {
                return !(details.DataContext is IDataErrorInfo) ||
                       string.IsNullOrEmpty((details.DataContext as IDataErrorInfo).Error);
            }
        }

        #region IUdpListenerDetails Members

        public bool Enabled
        {
            get
            {
                return enabled;
            }
        }

        public string LogViewerName
        {
            get
            {
                return logViewerName;
            }
        }

        public int Port
        {
            get
            {
                return port;
            }
        }

        #endregion

        private void CloseDialog(bool dialogResult)
        {
            DialogResult = dialogResult;

            if (dialogResult
                && details.DataContext != null
                && details.DataContext is IUdpListenerDetails)
            {
                IUdpListenerDetails listenerDetails = (IUdpListenerDetails) details.DataContext;
                logViewerName = listenerDetails.LogViewerName;
                port = listenerDetails.Port;
                enabled = listenerDetails.Enabled;
            }

            Close();
        }
    }
}