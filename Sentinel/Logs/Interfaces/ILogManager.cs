namespace Sentinel.Logs.Interfaces
{
    using System.Collections.Generic;

    using Sentinel.Interfaces;

    public interface ILogManager : IEnumerable<ILogger>
    {
        ILogger Add(string logName);

        ILogger Get(string name);

        void Remove(string name);
    }
}