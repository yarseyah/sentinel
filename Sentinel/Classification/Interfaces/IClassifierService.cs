using System.Collections.ObjectModel;
using System.Windows.Input;
using Sentinel.Interfaces;

namespace Sentinel.Classification.Interfaces
{
    public interface IClassifierService
    {
        ICommand Add { get; }

        ICommand Edit { get; }

        ObservableCollection<IClassifier> Items { get; }

        ICommand OrderEarlier { get; }

        ICommand OrderLater { get; }

        ICommand Remove { get; }

        int SelectedIndex { get; set; }

        LogEntry Classify(LogEntry entry);
    }
}