#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

using System.Windows.Controls;

#endregion

namespace Sentinel.Logger
{
    public interface ILogViewer
    {
        string Name { get; }

        Control Presenter { get; }
    }
}