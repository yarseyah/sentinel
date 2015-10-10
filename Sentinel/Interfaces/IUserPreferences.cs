namespace Sentinel.Interfaces
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    public interface IUserPreferences
    {
        string CurrentThemeName { get; }

        IEnumerable<string> DateFormatOptions { get; }

        IEnumerable<string> DateSourceOptions { get; }
            
        [DataMember]
        int SelectedDateOption { get; set; }

        [DataMember]
        int DateSourceOption { get; set; }

        [DataMember]
        int SelectedTypeOption { get; set; }

        bool Show { get; set; }

        [DataMember]
        bool ShowThreadColumn { get; set; }

        [DataMember]
        bool ShowExceptionColumn { get; set; }

        IEnumerable<string> TypeOptions { get; }

        [DataMember]
        bool UseLazyRebuild { get; set; }

        [DataMember]
        bool UseStackedLayout { get; set; }

        [DataMember]
        bool UseTighterRows { get; set; }

        [DataMember]
        bool DoubleClickToShowExceptions { get; set; }

        [DataMember]
        bool ShowSourceInformationColumns { get; set; }
    }
}