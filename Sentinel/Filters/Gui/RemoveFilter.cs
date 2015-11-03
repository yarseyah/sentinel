namespace Sentinel.Filters.Gui
{
    using System.Windows;

    using Sentinel.Filters.Interfaces;
    using Sentinel.Services;

    public class RemoveFilter : IRemoveFilterService
    {
        public void Remove(IFilter filter)
        {
            var service = ServiceLocator.Instance.Get<IFilteringService<IFilter>>();

            if (service != null)
            {
                var prompt = "Are you sure you want to remove the selected filter?\r\n\r\n" +
                                $"Filter Name = \"{filter.Name}\"";

                var result = MessageBox.Show(
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
    }
}