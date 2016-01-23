namespace Sentinel.Views.Interfaces
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows.Controls;

    using Sentinel.Interfaces;

    public interface ILogViewer
    {
        string Name { get; }

        ILogger Logger { get; }

        Control Presenter { get; }

        ObservableCollection<ILogEntry> Messages { get; }

        string Status { get; }

        IEnumerable<ILogViewerToolbarButton> ToolbarItems { get; }

        void SetLogger(ILogger newLogger);
    }
}