#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;

#endregion

namespace Sentinel.Interfaces
{

    #region Using directives

    #endregion

    public delegate string GetFriendlyNameDelegate<T>(T obj);

    public class CollectionChangeHelper<T>
    {
        public string ManagerName { get; set; }

        public event GetFriendlyNameDelegate<T> NameLookup;

        public event PropertyChangedEventHandler OnPropertyChanged;

        public void AttachDetach(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (object newItem in e.NewItems)
                {
                    Debug.Assert(newItem != null, "New item to insert can not be null.");
                    Debug.Assert(newItem is T, "New item to insert must be a " + typeof(T));

                    if (e.NewItems != null
                        && newItem is T
                        && newItem is INotifyPropertyChanged)
                    {
                        // Register on OnPropertyChanged.
                        INotifyPropertyChanged notifyPropertyChanged = newItem as INotifyPropertyChanged;
                        T t = (T) newItem;
                        string name = NameLookup != null ? NameLookup(t) : "<Unknown>";

                        Trace.WriteLine(
                            string.Format(
                                "{0} detected {1} added to collection and binding to its PropertyChanged event",
                                ManagerName,
                                name));

                        notifyPropertyChanged.PropertyChanged += OnPropertyChanged;
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (object oldItem in e.OldItems)
                {
                    Debug.Assert(oldItem != null, "Item to remove can not be null.");
                    Debug.Assert(oldItem is T, "Item to remove must be a " + typeof(T));

                    if (e.OldItems != null
                        && oldItem is T
                        && oldItem is INotifyPropertyChanged)
                    {
                        // Unregister on OnPropertyChanged.
                        INotifyPropertyChanged notifyPropertyChanged = oldItem as INotifyPropertyChanged;
                        T t = (T) oldItem;
                        string name = NameLookup != null ? NameLookup(t) : "<Unknown>";

                        Trace.WriteLine(
                            string.Format(
                                "{0} detected {1} removed from collection and unbinding from its PropertyChanged event",
                                ManagerName,
                                name));

                        notifyPropertyChanged.PropertyChanged -= OnPropertyChanged;
                    }
                }
            }
        }
    }
}