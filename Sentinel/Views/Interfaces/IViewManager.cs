namespace Sentinel.Views.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public interface IViewManager
    {
        ObservableCollection<IWindowFrame> Viewers { get; }

        void Register(IViewInformation info, Type viewerType);

        IEnumerable<IViewInformation> GetRegistered();

        IViewInformation Get(string identifier);

        ILogViewer GetInstance(string identifier);
    }
}