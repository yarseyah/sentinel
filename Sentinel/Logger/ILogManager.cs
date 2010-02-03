#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

namespace Sentinel.Logger
{
    public interface ILogManager
    {
        void Add(ILogger logger);

        ILogger Get(string name);

        void Remove(string name);
    }
}