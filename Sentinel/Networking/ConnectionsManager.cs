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

#endregion

namespace Sentinel.Networking
{

    #region Using directives

    #endregion

    public class ConnectionsManager : IConnectionsManager
    {
        private readonly Dictionary<string, INetworkListener> listeners = new Dictionary<string, INetworkListener>();

        #region IConnectionsManager Members

        public void Add(INetworkListener listener)
        {
            if (listener != null)
            {
                if (listeners.ContainsKey(listener.Name))
                {
                    throw new ArgumentException("Duplicates are not supported");
                }

                listeners[listener.Name] = listener;
            }
            else
            {
                throw new ArgumentNullException("listener");
            }
        }

        public INetworkListener Get(string name)
        {
            return listeners[name];
        }

        public void Remove(string name)
        {
            if (listeners.ContainsKey(name))
            {
                listeners.Remove(name);
            }
        }

        #endregion
    }
}