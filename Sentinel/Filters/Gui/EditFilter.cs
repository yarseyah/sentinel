#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Sentinel.Filters.Interfaces;
using Sentinel.Interfaces;
using Sentinel.Support.Converters;

#endregion

namespace Sentinel.Filters.Gui
{
    public class EditFilter : IEditFilterService
    {
        #region IEditFilterService Members

        public void Edit(IFilter filter)
        {
            Debug.Assert(filter != null, "Filter must be supplied to allow editing.");

            AddEditFilterWindow window = new AddEditFilterWindow();
            AddEditFilter data = new AddEditFilter(window, true);
            window.DataContext = data;
            window.Owner = Application.Current.MainWindow;

            data.Name = filter.Name;
            data.FieldIndex = filter.Field == LogEntryField.Type ? 0 : 1;
            data.Pattern = filter.Pattern;
            data.FilterMethod = MatchModeConverter.Convert(filter.Mode, data.FilterMethods);

            bool? dialogResult = window.ShowDialog();

            if (dialogResult != null && (bool) dialogResult)
            {
                filter.Name = data.Name;
                filter.Pattern = data.Pattern;
                filter.Mode = MatchModeConverter.ConvertFrom(data.FilterMethods.ElementAt(data.FilterMethod));
                filter.Field = LogEntryFieldHelper.FieldNameToEnumeration(data.Fields.ElementAt(data.FieldIndex));
            }
        }

        #endregion
    }
}