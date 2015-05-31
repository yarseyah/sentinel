#region License

//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//

#endregion License

#region Using directives

using Sentinel.Interfaces;
using System.Collections.ObjectModel;

#endregion Using directives

namespace Sentinel.Filters.Interfaces
{
    using System.Runtime.Serialization;

    public interface IFilteringService<T>
    {
        [DataMember]
        ObservableCollection<T> Filters { get; set; }

        ObservableCollection<T> SearchFilters { get; set; }

        bool IsFiltered(ILogEntry entry);
    }
}