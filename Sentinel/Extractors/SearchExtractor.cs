using Sentinel.Extractors.Interfaces;
using Sentinel.Interfaces;
using System.Runtime.Serialization;

namespace Sentinel.Extractors
{
    [DataContract]
    public class SearchExtractor
        : Extractor, IDefaultInitialisation, ISearchExtractor
    {
        public SearchExtractor()
        {
        }

        public void Initialise()
        {
            base.Name = "SearchExtractor";
            base.Field = LogEntryField.System;
            base.Pattern = string.Empty;
        }
    }
}