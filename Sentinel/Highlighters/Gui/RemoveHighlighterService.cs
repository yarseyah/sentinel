namespace Sentinel.Highlighters.Gui
{
    using System.Windows;

    using Interfaces;
    using Services;

    public class RemoveHighlighterService : IRemoveHighlighterService
    {
        public void Remove(IHighlighter highlighter)
        {
            var service = ServiceLocator.Instance.Get<IHighlightingService<IHighlighter>>();
            if (service == null)
            {
                return;
            }

            var prompt =
                $@"Are you sure you want to remove the selected highlighter?

Highlighter Name = ""{highlighter.Name}""";

            var result = MessageBox.Show(
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
}