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
using Sentinel.Highlighters.Interfaces;
using Sentinel.Services;

#endregion

namespace Sentinel.Highlighters.Gui
{
    [Export(typeof(IRemoveHighlighterService))]
    public class RemoveHighlighterService : IRemoveHighlighterService
    {
        #region IRemoveHighlighterService Members

        public void Remove(IHighlighter highlighter)
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