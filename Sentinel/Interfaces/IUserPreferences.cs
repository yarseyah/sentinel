#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

namespace Sentinel.Interfaces
{
    #region Using directives
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    #endregion

    public interface IUserPreferences
    {
        string CurrentThemeName { get; }

        IEnumerable<string> DateFormatOptions { get; }

        [DataMember]
        int SelectedDateOption { get; set; }

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
    }
}