namespace Sentinel.Interfaces
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    public interface IUserPreferences
    {
        string CurrentThemeName { get; }

        IEnumerable<string> DateFormatOptions { get; }

        IEnumerable<string> TimeFormatOptions { get; }

        [DataMember]
        int SelectedDateOption { get; set; }

        [DataMember]
        int SelectedTimeFormatOption { get; set; }

        [DataMember]
        bool ConvertUtcTimesToLocalTimeZone { get; set; }

        [DataMember]
        bool UseArrivalDateTime { get; set; }

        [DataMember]
        int SelectedTypeOption { get; set; }

        bool Show { get; set; }

        [DataMember]
        bool ShowThreadColumn { get; set; }

        [DataMember]
        bool ShowExceptionColumn { get; set; }

        [DataMember]
        bool ShowSourceColumn { get; set; }

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