using System.Collections.ObjectModel;
using System.Windows.Input;
using Sentinel.Interfaces;
using System.Runtime.Serialization;

namespace Sentinel.Classification.Interfaces
{
    public interface IClassifyingService<T>
    {
        ICommand Add { get; }

        ICommand Edit { get; }

        [DataMember]
        ObservableCollection<T> Classifiers { get; }

        ICommand OrderEarlier { get; }

        ICommand OrderLater { get; }

        ICommand Remove { get; }

        int SelectedIndex { get; set; }

        ILogEntry Classify(ILogEntry entry);
    }
}