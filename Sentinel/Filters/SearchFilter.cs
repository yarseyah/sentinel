namespace Sentinel.Filters
{
    using System.Runtime.Serialization;

    using Sentinel.Filters.Interfaces;
    using Sentinel.Interfaces;

    [DataContract]
    public class SearchFilter : Filter, IDefaultInitialisation, ISearchFilter
    {
        public void Initialise()
        {
            Name = "SearchFilter";
            Field = LogEntryField.System;
            Pattern = string.Empty;
        }
    }
}