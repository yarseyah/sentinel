using System.Collections.Generic;
using Sentinel.Logs.Interfaces;

namespace Sentinel.Views.Interfaces
{
    using Sentinel.Interfaces;

    public interface IWindowFrame
    {
        ILogger Log { get; set; }
        ILogViewer PrimaryView { get; set; }
        void SetViews(IEnumerable<string> viewIdentifiers);
    }
}