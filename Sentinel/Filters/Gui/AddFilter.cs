#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

using System.Linq;
using System.Windows;
using Sentinel.Filters.Interfaces;
using Sentinel.Interfaces;
using Sentinel.Services;
using Sentinel.Support.Converters;

#endregion

namespace Sentinel.Filters.Gui
{
    public class AddFilter
        : IAddFilterService
    {
        #region IAddFilterService Members

        public void Add()
        {
            AddEditFilterWindow filterWindow = new AddEditFilterWindow();
            using (AddEditFilter data = new AddEditFilter(filterWindow, false))
            {
                filterWindow.DataContext = data;
                filterWindow.Owner = Application.Current.MainWindow;
                bool? dialogResult = filterWindow.ShowDialog();
                if (dialogResult != null && (bool)dialogResult)
                {
                    Filter filter = Construct(data);
                    if (filter != null)
                    {
                        var service = ServiceLocator.Instance.Get<IFilteringService<IFilter>>();
                        if (service != null)
                        {
                            service.Filters.Add(filter);
                        }
                    }
                }
            }
        }

        #endregion

        private static Filter Construct(AddEditFilter data)
        {
            return new Filter
            {
                Name = data.Name,
                Field = data.Field,
                Mode = data.Mode,
                Pattern = data.Pattern,
                Enabled = true
            };
        }
    }
}