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
using System.Collections.ObjectModel;

#endregion

namespace Sentinel.Views.Interfaces
{
    public interface IViewManager
    {
        ObservableCollection<IWindowFrame> Viewers { get; }
        void Register(IViewInformation info, Type viewerType);
        IEnumerable<IViewInformation> GetRegistered();
        IViewInformation Get(string identifier);
        ILogViewer GetInstance(string identifier);
    }
}