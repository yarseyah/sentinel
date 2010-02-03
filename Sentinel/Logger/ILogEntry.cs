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

#endregion

namespace Sentinel.Logger
{
    public interface ILogEntry
    {
        string Classification { get; set; }

        DateTime DateTime { get; set; }

        string Description { get; set; }

        string Source { get; set; }

        string System { get; set; }

        string Thread { get; set; }

        string Type { get; set; }
    }
}