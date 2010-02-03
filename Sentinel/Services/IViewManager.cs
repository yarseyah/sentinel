#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

using System.Collections.ObjectModel;
using Sentinel.Logger;

#endregion

namespace Sentinel.Services
{

    #region Using directives

    #endregion

    public interface IViewManager
    {
        ObservableCollection<ILogViewer> Viewers { get; }

        void Create(string name, int port, bool enabled);

        LogViewerState GetState(string name);

        void SetState(string name, LogViewerState state);
    }
}