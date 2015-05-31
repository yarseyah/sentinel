#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

using System.Collections.Generic;
using System.Windows.Controls;
using Sentinel.Logs.Interfaces;

#endregion

namespace Sentinel.Views.Interfaces
{
    using Sentinel.Interfaces;
    using System.Collections.ObjectModel;

    public interface ILogViewer
    {
        string Name { get; }

        ILogger Logger { get; }

        Control Presenter { get; }

        ObservableCollection<ILogEntry> Messages { get; }

        string Status { get; }

        void SetLogger(ILogger newLogger);

        IEnumerable<ILogViewerToolbarButton> ToolbarItems { get; }
    }
}