using System.Collections.Generic;
using Sentinel.Logs.Interfaces;

namespace Sentinel.Views.Interfaces
{
    public interface IWindowFrame
    {
        ILogger Log { get; set; }
        void SetViews(IEnumerable<string> viewIdentifiers);
    }
}