namespace Sentinel.Filters.Interfaces
{
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;

    using Sentinel.Interfaces;

    public interface IFilteringService<T>
    {
        [DataMember]
        ObservableCollection<T> Filters { get; set; }

        ObservableCollection<T> SearchFilters { get; set; }

        bool IsFiltered(ILogEntry entry);
    }
}