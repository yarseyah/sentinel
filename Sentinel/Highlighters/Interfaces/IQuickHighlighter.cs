#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

using Sentinel.Interfaces;

#endregion

namespace Sentinel.Highlighters.Interfaces
{
    public interface IQuickHighlighter
    {
        LogEntryField Field { get; set; }

        Highlighter Highlighter { get; }

        string Search { get; set; }
    }
}