#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

using System.Collections.Generic;

namespace Sentinel.Logs.Interfaces
{
    using Sentinel.Interfaces;

    public interface ILogManager : IEnumerable<ILogger>
    {
        ILogger Add(string logName);

        ILogger Get(string name);

        void Remove(string name);
    }
}