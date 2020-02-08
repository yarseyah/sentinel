namespace Sentinel.Support.Wpf
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using Sentinel.Interfaces.CodeContracts;

    [Serializable]
    public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>,
                                                      IDictionary,
                                                      ISerializable,
                                                      IDeserializationCallback,
                                                      INotifyCollectionChanged,
                                                      INotifyPropertyChanged
    {
        [NonSerialized]
        private readonly SerializationInfo serializationInfo;

        private readonly Dictionary<TKey, TValue> dictionaryCache = new Dictionary<TKey, TValue>();

        private int countCache;

        private int dictionaryCacheVersion;

        private int version;

        public ObservableDictionary()
        {
            KeyedEntryCollection = new KeyedDictionaryEntryCollection();
        }

        public ObservableDictionary(IDictionary<TKey, TValue> dictionary)
        {
            dictionary.ThrowIfNull(nameof(dictionary));

            KeyedEntryCollection = new KeyedDictionaryEntryCollection();

            foreach (var entry in dictionary)
            {
                DoAddEntry(entry.Key, entry.Value);
            }
        }

        public ObservableDictionary(IEqualityComparer<TKey> comparer)
        {
            KeyedEntryCollection = new KeyedDictionaryEntryCollection(comparer);
        }

        public ObservableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
        {
            dictionary.ThrowIfNull(nameof(dictionary));

            KeyedEntryCollection = new KeyedDictionaryEntryCollection(comparer);

            foreach (var entry in dictionary)
            {
                DoAddEntry(entry.Key, entry.Value);
            }
        }

        protected ObservableDictionary(SerializationInfo info, StreamingContext context)
        {
            serializationInfo = info;
        }

        protected virtual event NotifyCollectionChangedEventHandler CollectionChanged;

        protected virtual event PropertyChangedEventHandler PropertyChanged;

        event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
        {
            add
            {
                CollectionChanged += value;
            }

            remove
            {
                CollectionChanged -= value;
            }
        }

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                PropertyChanged += value;
            }

            remove
            {
                PropertyChanged -= value;
            }
        }

        public IEqualityComparer<TKey> Comparer => KeyedEntryCollection.Comparer;

        public int Count => KeyedEntryCollection.Count;

        public Dictionary<TKey, TValue>.KeyCollection Keys => TrueDictionary.Keys;

        public Dictionary<TKey, TValue>.ValueCollection Values => TrueDictionary.Values;

        int ICollection.Count => KeyedEntryCollection.Count;

        bool IDictionary.IsFixedSize => false;

        bool IDictionary.IsReadOnly => false;

        bool ICollection.IsSynchronized => ((ICollection)KeyedEntryCollection).IsSynchronized;

        ICollection IDictionary.Keys => Keys;

        object ICollection.SyncRoot => ((ICollection)KeyedEntryCollection).SyncRoot;

        ICollection IDictionary.Values => Values;

        int ICollection<KeyValuePair<TKey, TValue>>.Count => KeyedEntryCollection.Count;

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;

        ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;

        protected KeyedDictionaryEntryCollection KeyedEntryCollection { get; }

        private Dictionary<TKey, TValue> TrueDictionary
        {
            get
            {
                if (dictionaryCacheVersion != version)
                {
                    dictionaryCache.Clear();
                    foreach (var entry in KeyedEntryCollection)
                    {
                        dictionaryCache.Add((TKey)entry.Key, (TValue)entry.Value);
                    }

                    dictionaryCacheVersion = version;
                }

                return dictionaryCache;
            }
        }

        public TValue this[TKey key]
        {
            get => (TValue)KeyedEntryCollection[key].Value;
            set => DoSetEntry(key, value);
        }

        object IDictionary.this[object key]
        {
            get => KeyedEntryCollection[(TKey)key].Value;
            set => DoSetEntry((TKey)key, (TValue)value);
        }

        TValue IDictionary<TKey, TValue>.this[TKey key]
        {
            get => (TValue)KeyedEntryCollection[key].Value;
            set => DoSetEntry(key, value);
        }

        public void Add(TKey key, TValue value)
        {
            DoAddEntry(key, value);
        }

        public void Clear()
        {
            DoClearEntries();
        }

        public bool ContainsKey(TKey key) => KeyedEntryCollection.Contains(key);

        public bool ContainsValue(TValue value) => TrueDictionary.ContainsValue(value);

        public IEnumerator GetEnumerator() => new Enumerator(this, false);

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.ThrowIfNull(nameof(info));

            var entries = new Collection<DictionaryEntry>();
            foreach (var entry in KeyedEntryCollection)
            {
                entries.Add(entry);
            }

            info.AddValue("entries", entries);
        }

        public virtual void OnDeserialization(object sender)
        {
            if (serializationInfo != null)
            {
                var entries = (Collection<DictionaryEntry>)serializationInfo.GetValue("entries", typeof(Collection<DictionaryEntry>));
                foreach (var entry in entries)
                {
                    AddEntry((TKey)entry.Key, (TValue)entry.Value);
                }
            }
        }

        public bool Remove(TKey key) => DoRemoveEntry(key);

        public bool TryGetValue(TKey key, out TValue value)
        {
            var result = KeyedEntryCollection.Contains(key);
            value = result ? (TValue)KeyedEntryCollection[key].Value : default(TValue);
            return result;
        }

        void IDictionary.Add(object key, object value)
        {
            DoAddEntry((TKey)key, (TValue)value);
        }

        void IDictionary.Clear()
        {
            DoClearEntries();
        }

        bool IDictionary.Contains(object key)
        {
            return KeyedEntryCollection.Contains((TKey)key);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)KeyedEntryCollection).CopyTo(array, index);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new Enumerator(this, true);
        }

        void IDictionary.Remove(object key)
        {
            DoRemoveEntry((TKey)key);
        }

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            DoAddEntry(key, value);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> kvp)
        {
            DoAddEntry(kvp.Key, kvp.Value);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
            DoClearEntries();
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> kvp)
        {
            return KeyedEntryCollection.Contains(kvp.Key);
        }

        bool IDictionary<TKey, TValue>.ContainsKey(TKey key)
        {
            return KeyedEntryCollection.Contains(key);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            array.ThrowIfNull(nameof(array));

            if (index < 0 || index > array.Length)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(index),
                    "CopyTo() failed:  index parameter was outside the bounds of the supplied array");
            }

            if (array.Length - index < KeyedEntryCollection.Count)
            {
                throw new ArgumentException("CopyTo() failed:  supplied array was too small", nameof(array));
            }

            foreach (var entry in KeyedEntryCollection)
            {
                array[index++] = new KeyValuePair<TKey, TValue>((TKey)entry.Key, (TValue)entry.Value);
            }
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return new Enumerator(this, false);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            return DoRemoveEntry(key);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> kvp)
        {
            return DoRemoveEntry(kvp.Key);
        }

        bool IDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value)
        {
            return TryGetValue(key, out value);
        }

        protected virtual bool AddEntry(TKey key, TValue value)
        {
            KeyedEntryCollection.Add(new DictionaryEntry(key, value));
            return true;
        }

        protected virtual bool ClearEntries()
        {
            // check whether there are entries to clear
            var result = Count > 0;
            if (result)
            {
                // if so, clear the dictionary
                KeyedEntryCollection.Clear();
            }

            return result;
        }

        protected int GetIndexAndEntryForKey(TKey key, out DictionaryEntry entry)
        {
            entry = default(DictionaryEntry);
            var index = -1;
            if (KeyedEntryCollection.Contains(key))
            {
                entry = KeyedEntryCollection[key];
                index = KeyedEntryCollection.IndexOf(entry);
            }

            return index;
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            CollectionChanged?.Invoke(this, args);
        }

        protected virtual void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected virtual bool RemoveEntry(TKey key)
        {
            // remove the entry
            return KeyedEntryCollection.Remove(key);
        }

        protected virtual bool SetEntry(TKey key, TValue value)
        {
            var keyExists = KeyedEntryCollection.Contains(key);

            // if identical key/value pair already exists, nothing to do
            if (keyExists && value.Equals((TValue)KeyedEntryCollection[key].Value))
            {
                return false;
            }

            // otherwise, remove the existing entry
            if (keyExists)
            {
                KeyedEntryCollection.Remove(key);
            }

            // add the new entry
            KeyedEntryCollection.Add(new DictionaryEntry(key, value));

            return true;
        }

        private void DoAddEntry(TKey key, TValue value)
        {
            if (AddEntry(key, value))
            {
                version++;

                DictionaryEntry entry;
                var index = GetIndexAndEntryForKey(key, out entry);
                FireEntryAddedNotifications(entry, index);
            }
        }

        private void DoClearEntries()
        {
            if (ClearEntries())
            {
                version++;
                FireResetNotifications();
            }
        }

        private bool DoRemoveEntry(TKey key)
        {
            DictionaryEntry entry;
            var index = GetIndexAndEntryForKey(key, out entry);

            var result = RemoveEntry(key);
            if (result)
            {
                version++;
                if (index > -1)
                {
                    FireEntryRemovedNotifications(entry, index);
                }
            }

            return result;
        }

        private void DoSetEntry(TKey key, TValue value)
        {
            DictionaryEntry entry;
            var index = GetIndexAndEntryForKey(key, out entry);

            if (SetEntry(key, value))
            {
                version++;

                // if prior entry existed for this key, fire the removed notifications
                if (index > -1)
                {
                    FireEntryRemovedNotifications(entry, index);

                    // force the property change notifications to fire for the modified entry
                    countCache--;
                }

                // then fire the added notifications
                index = GetIndexAndEntryForKey(key, out entry);
                FireEntryAddedNotifications(entry, index);
            }
        }

        private void FireEntryAddedNotifications(DictionaryEntry entry, int index)
        {
            // fire the relevant PropertyChanged notifications
            FirePropertyChangedNotifications();

            // fire CollectionChanged notification
            if (index > -1)
            {
                OnCollectionChanged(
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Add,
                        new KeyValuePair<TKey, TValue>((TKey)entry.Key, (TValue)entry.Value),
                        index));
            }
            else
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        private void FireEntryRemovedNotifications(DictionaryEntry entry, int index)
        {
            // fire the relevant PropertyChanged notifications
            FirePropertyChangedNotifications();

            // fire CollectionChanged notification
            if (index > -1)
            {
                OnCollectionChanged(
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Remove,
                        new KeyValuePair<TKey, TValue>((TKey)entry.Key, (TValue)entry.Value),
                        index));
            }
            else
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        private void FirePropertyChangedNotifications()
        {
            if (Count != countCache)
            {
                countCache = Count;
                OnPropertyChanged("Count");
                OnPropertyChanged("Item[]");
                OnPropertyChanged("Keys");
                OnPropertyChanged("Values");
            }
        }

        private void FireResetNotifications()
        {
            // fire the relevant PropertyChanged notifications
            FirePropertyChangedNotifications();

            // fire CollectionChanged notification
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDictionaryEnumerator
        {
            private readonly ObservableDictionary<TKey, TValue> dictionary;

            private readonly int version;

            private readonly bool isDictionaryEntryEnumerator;

            private int index;

            private KeyValuePair<TKey, TValue> current;

            internal Enumerator(ObservableDictionary<TKey, TValue> dictionary, bool isDictionaryEntryEnumerator)
            {
                this.dictionary = dictionary;
                version = dictionary.version;
                index = -1;
                this.isDictionaryEntryEnumerator = isDictionaryEntryEnumerator;
                current = default(KeyValuePair<TKey, TValue>);
            }

            object IEnumerator.Current
            {
                get
                {
                    ValidateCurrent();
                    if (isDictionaryEntryEnumerator)
                    {
                        return new DictionaryEntry(current.Key, current.Value);
                    }

                    return new KeyValuePair<TKey, TValue>(current.Key, current.Value);
                }
            }

            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    ValidateCurrent();
                    return current;
                }
            }

            object IDictionaryEnumerator.Key
            {
                get
                {
                    ValidateCurrent();
                    return current.Key;
                }
            }

            DictionaryEntry IDictionaryEnumerator.Entry
            {
                get
                {
                    ValidateCurrent();
                    return new DictionaryEntry(current.Key, current.Value);
                }
            }

            object IDictionaryEnumerator.Value
            {
                get
                {
                    ValidateCurrent();
                    return current.Value;
                }
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                ValidateVersion();
                index++;
                if (index < dictionary.KeyedEntryCollection.Count)
                {
                    current = new KeyValuePair<TKey, TValue>(
                        (TKey)dictionary.KeyedEntryCollection[index].Key,
                        (TValue)dictionary.KeyedEntryCollection[index].Value);
                    return true;
                }

                index = -2;
                current = default(KeyValuePair<TKey, TValue>);
                return false;
            }

            void IEnumerator.Reset()
            {
                ValidateVersion();
                index = -1;
                current = default(KeyValuePair<TKey, TValue>);
            }

            private void ValidateVersion()
            {
                if (version != dictionary.version)
                {
                    throw new InvalidOperationException("The enumerator is not valid because the dictionary changed.");
                }
            }

            private void ValidateCurrent()
            {
                switch (index)
                {
                    case -1:
                        throw new InvalidOperationException("The enumerator has not been started.");
                    case -2:
                        throw new InvalidOperationException("The enumerator has reached the end of the collection.");
                }
            }
        }

        protected class KeyedDictionaryEntryCollection : KeyedCollection<TKey, DictionaryEntry>
        {
            public KeyedDictionaryEntryCollection()
            {
            }

            public KeyedDictionaryEntryCollection(IEqualityComparer<TKey> comparer)
                : base(comparer)
            {
            }

            protected override TKey GetKeyForItem(DictionaryEntry entry)
            {
                return (TKey)entry.Key;
            }
        }
    }
}