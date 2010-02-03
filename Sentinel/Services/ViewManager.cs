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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Sentinel.Controls;
using Sentinel.Logger;
using Sentinel.Networking;

#endregion

namespace Sentinel.Services
{

    #region Using directives

    #endregion

    public class ViewManager : IViewManager
    {
        private readonly Dictionary<string, LogViewerState> states = new Dictionary<string, LogViewerState>();

        public ViewManager()
        {
            Viewers = new ObservableCollection<ILogViewer>();
        }

        #region IViewManager Members

        public ObservableCollection<ILogViewer> Viewers { get; private set; }

        public void Create(string name, int port, bool enabled)
        {
            Debug.Assert(
                Viewers != null,
                "Viewers collection should have been created in constructor");
            Debug.Assert(
                Viewers.Any(v => v.Name == name) == false,
                "Newly created viewer should have a unique name.");

            ILogManager logManager = ServiceLocator.Instance.Get<ILogManager>();
            IConnectionsManager connectionsManager = ServiceLocator.Instance.Get<IConnectionsManager>();

            logManager.Add(new Log(name));
            connectionsManager.Add(new UdpListener(name, port));
            LogDetailsControl control = new LogDetailsControl(name);

            IUdpLogViewer viewer = new LogViewer(name, control) {Port = port};
            Viewers.Add(viewer);
        }

        public LogViewerState GetState(string name)
        {
            ILogViewerWithState viewer = Viewers
                .OfType<ILogViewerWithState>()
                .FirstOrDefault(v => v.Name == name);

            if (viewer != null)
            {
                return viewer.State;
            }

            return states.ContainsKey(name) ? states[name] : LogViewerState.Unknown;
        }

        public void SetState(string name, LogViewerState state)
        {
            ILogViewerWithState viewer = Viewers
                .OfType<ILogViewerWithState>()
                .FirstOrDefault(v => v.Name == name);
            if (viewer != null)
            {
                viewer.State = state;
            }

            states[name] = state;
        }

        #endregion
    }
}