namespace Sentinel.Extractors
{
    using System.Runtime.Serialization;

    using Sentinel.Extractors.Interfaces;
    using Sentinel.Interfaces;

    [DataContract]
    public class SearchExtractor
        : Extractor, IDefaultInitialisation, ISearchExtractor
    {
        public void Initialise()
        {
            Name = "SearchExtractor";
            Field = LogEntryField.System;
            Pattern = string.Empty;
        }
    }
}