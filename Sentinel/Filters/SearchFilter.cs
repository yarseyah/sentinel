namespace Sentinel.Filters
{
    #region Using directives

    using System.Runtime.Serialization;

    using Sentinel.Filters.Interfaces;
    using Sentinel.Interfaces;

    #endregion Using directives

    [DataContract]
    public class SearchFilter
        : Filter, IDefaultInitialisation, ISearchFilter
    {
        public void Initialise()
        {
            Name = "SearchFilter";
            Field = LogEntryField.System;
            Pattern = string.Empty;
        }     
    }
}