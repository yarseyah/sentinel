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
using System.Windows;
using Sentinel.Filters.Interfaces;
using Sentinel.Services;

#endregion

namespace Sentinel.Filters.Gui
{
    #region Using directives

    #endregion

    public class RemoveFilter : IRemoveFilterService
    {
        #region IRemoveFilterService Members

        public void Remove(IFilter filter)
        {
            IFilteringService service = ServiceLocator.Instance.Get<IFilteringService>();

            if (service != null)
            {
                string prompt = string.Format(
                    "Are you sure you want to remove the selected filter?\r\n\r\n" +
                    "Filter Name = \"{0}\"",
                    filter.Name);

                MessageBoxResult result = MessageBox.Show(
                    prompt,
                    "Remove Filter",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.No);

                if (result == MessageBoxResult.Yes)
                {
                    service.Filters.Remove(filter);
                }
            }
        }

        #endregion
    }
}