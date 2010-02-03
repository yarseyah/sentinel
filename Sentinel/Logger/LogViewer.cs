#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

using System.Windows.Controls;
using Sentinel.Support;

#endregion

namespace Sentinel.Logger
{

    #region Using directives

    #endregion

    public class LogViewer
        : ViewModelBase, ILogViewerWithState, IUdpLogViewer
    {
        private string name;

        private int port;

        private Control presenter;

        private LogViewerState state;

        public LogViewer(string name, Control presenter)
        {
            Name = name;
            Presenter = presenter;
        }

        #region ILogViewerWithState Members

        /// <summary>
        /// Gets or sets the name of a LogViewer.
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        /// <summary>
        /// Gets or sets the Presenter control for a log viewer.
        /// </summary>
        public Control Presenter
        {
            get
            {
                return presenter;
            }

            set
            {
                if (presenter != value)
                {
                    presenter = value;
                    OnPropertyChanged("Presenter");
                }
            }
        }

        /// <summary>
        /// Gets or sets the State of the LogViewer.
        /// </summary>
        public LogViewerState State
        {
            get
            {
                return state;
            }

            set
            {
                if (state != value)
                {
                    state = value;
                    OnPropertyChanged("State");
                }
            }
        }

        #endregion

        #region IUdpLogViewer Members

        /// <summary>
        /// Gets or sets the Port for a log viewer.
        /// </summary>
        public int Port
        {
            get
            {
                return port;
            }

            set
            {
                if (port != value)
                {
                    port = value;
                    OnPropertyChanged("Port");
                }
            }
        }

        #endregion
    }
}