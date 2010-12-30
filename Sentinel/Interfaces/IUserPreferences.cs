#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

using System.Collections.Generic;

#endregion

namespace Sentinel.Interfaces
{

    #region Using directives

    #endregion

    public interface IUserPreferences
    {
        string CurrentThemeName { get; }

        IEnumerable<string> DateFormatOptions { get; }

        int SelectedDateOption { get; set; }

        int SelectedTypeOption { get; set; }

        bool Show { get; set; }

        bool ShowThreadColumn { get; set; }

        IEnumerable<string> TypeOptions { get; }

        bool UseLazyRebuild { get; set; }

        bool UseStackedLayout { get; set; }

        bool UseTighterRows { get; set; }
    }
}