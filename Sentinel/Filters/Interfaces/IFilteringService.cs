#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

using System.Collections.ObjectModel;
using Sentinel.Interfaces;

#endregion

namespace Sentinel.Filters.Interfaces
{
    using System.Runtime.Serialization;

    public interface IFilteringService<T>
    {
        [DataMember]
        ObservableCollection<T> Filters { get; set; }

        bool IsFiltered(LogEntry entry);
    }
}