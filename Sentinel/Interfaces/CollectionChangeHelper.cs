namespace Sentinel.Interfaces
{
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;

    using Common.Logging;

    public delegate string GetFriendlyNameDelegate<T>(T obj);

    public class CollectionChangeHelper<T>
    {
        public event GetFriendlyNameDelegate<T> NameLookup;

        public event PropertyChangedEventHandler OnPropertyChanged;

        public string ManagerName { get; set; }

        public void AttachDetach(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var newItem in e.NewItems)
                {
                    Debug.Assert(newItem != null, "New item to insert can not be null.");
                    Debug.Assert(newItem is T, "New item to insert must be a " + typeof(T));

                    if (e.NewItems != null && newItem is INotifyPropertyChanged)
                    {
                        // Register on OnPropertyChanged.
                        var notifyPropertyChanged = newItem as INotifyPropertyChanged;
                        var t = (T)newItem;
                        var name = NameLookup != null ? NameLookup(t) : "<Unknown>";

                        var log = LogManager.GetLogger("ObservableCollection:" + ManagerName);
                        log.DebugFormat(
                            "{0} detected {1} added to collection and binding to its PropertyChanged event",
                            ManagerName,
                            name);

                        notifyPropertyChanged.PropertyChanged += OnPropertyChanged;
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var oldItem in e.OldItems)
                {
                    Debug.Assert(oldItem != null, "Item to remove can not be null.");
                    Debug.Assert(oldItem is T, "Item to remove must be a " + typeof(T));

                    if (e.OldItems != null
                        && oldItem is INotifyPropertyChanged)
                    {
                        // Unregister on OnPropertyChanged.
                        var notifyPropertyChanged = oldItem as INotifyPropertyChanged;
                        var t = (T)oldItem;
                        var name = NameLookup != null ? NameLookup(t) : "<Unknown>";

                        var log = LogManager.GetLogger("ObservableCollection:" + ManagerName);
                        log.DebugFormat(
                            "{0} detected {1} removed from collection and unbinding from its PropertyChanged event",
                            ManagerName,
                            name);

                        notifyPropertyChanged.PropertyChanged -= OnPropertyChanged;
                    }
                }
            }
        }
    }
}