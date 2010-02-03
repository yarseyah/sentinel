#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

using System.Windows;
using Sentinel.Services;

#endregion

namespace Sentinel.Highlighting
{

    #region Using directives

    #endregion

    public class RemoveHighlighterService : IRemoveHighlighterService
    {
        #region IRemoveHighlighterService Members

        public void Remove(Highlighter highlighter)
        {
            IHighlightingService service = ServiceLocator.Instance.Get<IHighlightingService>();

            if (service != null)
            {
                string prompt = string.Format(
                    "Are you sure you want to remove the selected highlighter?\r\n\r\n" +
                    "Highlighter Name = \"{0}\"",
                    highlighter.Name);

                MessageBoxResult result = MessageBox.Show(
                    prompt,
                    "Remove Highlighter",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.No);

                if (result == MessageBoxResult.Yes)
                {
                    service.Highlighters.Remove(highlighter);
                }
            }
        }

        #endregion
    }
}