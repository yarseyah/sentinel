#region Using directives

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

#endregion

namespace Sentinel.Support.Wpf
{
    [Serializable]
    public class ObservableDictionary<TKey, TValue>
        :
            IDictionary<TKey, TValue>,
            IDictionary,
            ISerializable,
            IDeserializationCallback,
            INotifyCollectionChanged,
            INotifyPropertyChanged
    {
        #region constructors

        #region public

        public ObservableDictionary()
        {
            keyedEntryCollection = new KeyedDictionaryEntryCollection();
        }

        public ObservableDictionary(IDictionary<TKey, TValue> dictionary)
        {
            keyedEntryCollection = new KeyedDictionaryEntryCollection();

            foreach (KeyValuePair<TKey, TValue> entry in dictionary)
                DoAddEntry((TKey) entry.Key, (TValue) entry.Value);
        }

        public ObservableDictionary(IEqualityComparer<TKey> comparer)
        {
            keyedEntryCollection = new KeyedDictionaryEntryCollection(comparer);
        }

        public ObservableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
        {
            keyedEntryCollection = new KeyedDictionaryEntryCollection(comparer);

            foreach (KeyValuePair<TKey, TValue> entry in dictionary)
                DoAddEntry((TKey) entry.Key, (TValue) entry.Value);
        }

        #endregion public

        #region protected

        protected ObservableDictionary(SerializationInfo info, StreamingContext context)
        {
            siInfo = info;
        }

        #endregion protected

        #endregion constructors

        #region properties

        #region public

        public IEqualityComparer<TKey> Comparer
        {
            get
            {
                return keyedEntryCollection.Comparer;
            }
        }

        public int Count
        {
            get
            {
                return keyedEntryCollection.Count;
            }
        }

        public Dictionary<TKey, TValue>.KeyCollection Keys
        {
            get
            {
                return TrueDictionary.Keys;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                return (TValue) keyedEntryCollection[key].Value;
            }
            set
            {
                DoSetEntry(key, value);
            }
        }

        public Dictionary<TKey, TValue>.ValueCollection Values
        {
            get
            {
                return TrueDictionary.Values;
            }
        }

        #endregion public

        #region private

        private Dictionary<TKey, TValue> TrueDictionary
        {
            get
            {
                if (dictionaryCacheVersion != version)
                {
                    dictionaryCache.Clear();
                    foreach (DictionaryEntry entry in keyedEntryCollection)
                        dictionaryCache.Add((TKey) entry.Key, (TValue) entry.Value);
                    dictionaryCacheVersion = version;
                }
                return dictionaryCache;
            }
        }

        #endregion private

        #endregion properties

        #region methods

        #region public

        public void Add(TKey key, TValue value)
        {
            DoAddEntry(key, value);
        }

        public void Clear()
        {
            DoClearEntries();
        }

        public bool ContainsKey(TKey key)
        {
            return keyedEntryCollection.Contains(key);
        }

        public bool ContainsValue(TValue value)
        {
            return TrueDictionary.ContainsValue(value);
        }

        public IEnumerator GetEnumerator()
        {
            return new Enumerator(this, false);
        }

        public bool Remove(TKey key)
        {
            return DoRemoveEntry(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            bool result = keyedEntryCollection.Contains(key);
            value = result ? (TValue) keyedEntryCollection[key].Value : default(TValue);
            return result;
        }

        #endregion public

        #region protected

        protected virtual bool AddEntry(TKey key, TValue value)
        {
            keyedEntryCollection.Add(new DictionaryEntry(key, value));
            return true;
        }

        protected virtual bool ClearEntries()
        {
            // check whether there are entries to clear
            bool result = (Count > 0);
            if (result)
            {
                // if so, clear the dictionary
                keyedEntryCollection.Clear();
            }
            return result;
        }

        protected int GetIndexAndEntryForKey(TKey key, out DictionaryEntry entry)
        {
            entry = new DictionaryEntry();
            int index = -1;
            if (keyedEntryCollection.Contains(key))
            {
                entry = keyedEntryCollection[key];
                index = keyedEntryCollection.IndexOf(entry);
            }
            return index;
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, args);
        }

        protected virtual void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        protected virtual bool RemoveEntry(TKey key)
        {
            // remove the entry
            return keyedEntryCollection.Remove(key);
        }

        protected virtual bool SetEntry(TKey key, TValue value)
        {
            bool keyExists = keyedEntryCollection.Contains(key);

            // if identical key/value pair already exists, nothing to do
            if (keyExists && value.Equals((TValue) keyedEntryCollection[key].Value))
                return false;

            // otherwise, remove the existing entry
            if (keyExists)
                keyedEntryCollection.Remove(key);

            // add the new entry
            keyedEntryCollection.Add(new DictionaryEntry(key, value));

            return true;
        }

        #endregion protected

        #region private

        private void DoAddEntry(TKey key, TValue value)
        {
            if (AddEntry(key, value))
            {
                version++;

                DictionaryEntry entry;
                int index = GetIndexAndEntryForKey(key, out entry);
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
            int index = GetIndexAndEntryForKey(key, out entry);

            bool result = RemoveEntry(key);
            if (result)
            {
                version++;
                if (index > -1)
                    FireEntryRemovedNotifications(entry, index);
            }

            return result;
        }

        private void DoSetEntry(TKey key, TValue value)
        {
            DictionaryEntry entry;
            int index = GetIndexAndEntryForKey(key, out entry);

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
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                                                                         new KeyValuePair<TKey, TValue>(
                                                                             (TKey) entry.Key, (TValue) entry.Value),
                                                                         index));
            else
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void FireEntryRemovedNotifications(DictionaryEntry entry, int index)
        {
            // fire the relevant PropertyChanged notifications
            FirePropertyChangedNotifications();

            // fire CollectionChanged notification
            if (index > -1)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                                                                         new KeyValuePair<TKey, TValue>(
                                                                             (TKey) entry.Key, (TValue) entry.Value),
                                                                         index));
            else
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
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

        #endregion private

        #endregion methods

        #region interfaces

        #region IDictionary<TKey, TValue>

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            DoAddEntry(key, value);
        }

        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            return DoRemoveEntry(key);
        }

        bool IDictionary<TKey, TValue>.ContainsKey(TKey key)
        {
            return keyedEntryCollection.Contains(key);
        }

        bool IDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value)
        {
            return TryGetValue(key, out value);
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get
            {
                return Keys;
            }
        }

        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get
            {
                return Values;
            }
        }

        TValue IDictionary<TKey, TValue>.this[TKey key]
        {
            get
            {
                return (TValue) keyedEntryCollection[key].Value;
            }
            set
            {
                DoSetEntry(key, value);
            }
        }

        #endregion IDictionary<TKey, TValue>

        #region IDictionary

        void IDictionary.Add(object key, object value)
        {
            DoAddEntry((TKey) key, (TValue) value);
        }

        void IDictionary.Clear()
        {
            DoClearEntries();
        }

        bool IDictionary.Contains(object key)
        {
            return keyedEntryCollection.Contains((TKey) key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new Enumerator(this, true);
        }

        bool IDictionary.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        bool IDictionary.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        object IDictionary.this[object key]
        {
            get
            {
                return keyedEntryCollection[(TKey) key].Value;
            }
            set
            {
                DoSetEntry((TKey) key, (TValue) value);
            }
        }

        ICollection IDictionary.Keys
        {
            get
            {
                return Keys;
            }
        }

        void IDictionary.Remove(object key)
        {
            DoRemoveEntry((TKey) key);
        }

        ICollection IDictionary.Values
        {
            get
            {
                return Values;
            }
        }

        #endregion IDictionary

        #region ICollection<KeyValuePair<TKey, TValue>>

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
            return keyedEntryCollection.Contains(kvp.Key);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array", "CopyTo() failed:  array parameter was null");
            }
            if ((index < 0) || (index > array.Length))
            {
                throw new ArgumentOutOfRangeException("index",
                                                      "CopyTo() failed:  index parameter was outside the bounds of the supplied array");
            }
            if ((array.Length - index) < keyedEntryCollection.Count)
            {
                throw new ArgumentException("CopyTo() failed:  supplied array was too small", "array");
            }

            foreach (DictionaryEntry entry in keyedEntryCollection)
            {
                array[index++] = new KeyValuePair<TKey, TValue>((TKey) entry.Key, (TValue) entry.Value);
            }
        }

        int ICollection<KeyValuePair<TKey, TValue>>.Count
        {
            get
            {
                return keyedEntryCollection.Count;
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> kvp)
        {
            return DoRemoveEntry(kvp.Key);
        }

        #endregion ICollection<KeyValuePair<TKey, TValue>>

        #region ICollection

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection) keyedEntryCollection).CopyTo(array, index);
        }

        int ICollection.Count
        {
            get
            {
                return keyedEntryCollection.Count;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return ((ICollection) keyedEntryCollection).IsSynchronized;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return ((ICollection) keyedEntryCollection).SyncRoot;
            }
        }

        #endregion ICollection

        #region IEnumerable<KeyValuePair<TKey, TValue>>

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return new Enumerator(this, false);
        }

        #endregion IEnumerable<KeyValuePair<TKey, TValue>>

        #region IEnumerable

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion IEnumerable

        #region ISerializable

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            Collection<DictionaryEntry> entries = new Collection<DictionaryEntry>();
            foreach (DictionaryEntry entry in keyedEntryCollection)
                entries.Add(entry);
            info.AddValue("entries", entries);
        }

        #endregion ISerializable

        #region IDeserializationCallback

        public virtual void OnDeserialization(object sender)
        {
            if (siInfo == null) return;

            Collection<DictionaryEntry> entries = (Collection<DictionaryEntry>)
                                                  siInfo.GetValue("entries", typeof(Collection<DictionaryEntry>));
            foreach (DictionaryEntry entry in entries)
            {
                AddEntry((TKey) entry.Key, (TValue) entry.Value);
            }
        }

        #endregion IDeserializationCallback

        #region INotifyCollectionChanged

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

        protected virtual event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion INotifyCollectionChanged

        #region INotifyPropertyChanged

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

        protected virtual event PropertyChangedEventHandler PropertyChanged;

        #endregion INotifyPropertyChanged

        #endregion interfaces

        #region protected classes

        #region KeyedDictionaryEntryCollection<TKey>

        protected class KeyedDictionaryEntryCollection : KeyedCollection<TKey, DictionaryEntry>
        {
            #region constructors

            #region public

            public KeyedDictionaryEntryCollection()
            {
            }

            public KeyedDictionaryEntryCollection(IEqualityComparer<TKey> comparer)
                : base(comparer)
            {
            }

            #endregion public

            #endregion constructors

            #region methods

            #region protected

            protected override TKey GetKeyForItem(DictionaryEntry entry)
            {
                return (TKey) entry.Key;
            }

            #endregion protected

            #endregion methods
        }

        #endregion KeyedDictionaryEntryCollection<TKey>

        #endregion protected classes

        #region public structures

        #region Enumerator

        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDictionaryEnumerator
        {
            #region constructors

            internal Enumerator(ObservableDictionary<TKey, TValue> dictionary, bool isDictionaryEntryEnumerator)
            {
                this.dictionary = dictionary;
                version = dictionary.version;
                index = -1;
                this.isDictionaryEntryEnumerator = isDictionaryEntryEnumerator;
                current = new KeyValuePair<TKey, TValue>();
            }

            #endregion constructors

            #region properties

            #region public

            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    ValidateCurrent();
                    return current;
                }
            }

            #endregion public

            #endregion properties

            #region methods

            #region public

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                ValidateVersion();
                index++;
                if (index < dictionary.keyedEntryCollection.Count)
                {
                    current = new KeyValuePair<TKey, TValue>((TKey) dictionary.keyedEntryCollection[index].Key,
                                                             (TValue) dictionary.keyedEntryCollection[index].Value);
                    return true;
                }
                index = -2;
                current = new KeyValuePair<TKey, TValue>();
                return false;
            }

            #endregion public

            #region private

            private void ValidateCurrent()
            {
                if (index == -1)
                {
                    throw new InvalidOperationException("The enumerator has not been started.");
                }
                if (index == -2)
                {
                    throw new InvalidOperationException("The enumerator has reached the end of the collection.");
                }
            }

            private void ValidateVersion()
            {
                if (version != dictionary.version)
                {
                    throw new InvalidOperationException("The enumerator is not valid because the dictionary changed.");
                }
            }

            #endregion private

            #endregion methods

            #region IEnumerator implementation

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

            void IEnumerator.Reset()
            {
                ValidateVersion();
                index = -1;
                current = new KeyValuePair<TKey, TValue>();
            }

            #endregion IEnumerator implemenation

            #region IDictionaryEnumerator implemenation

            DictionaryEntry IDictionaryEnumerator.Entry
            {
                get
                {
                    ValidateCurrent();
                    return new DictionaryEntry(current.Key, current.Value);
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

            object IDictionaryEnumerator.Value
            {
                get
                {
                    ValidateCurrent();
                    return current.Value;
                }
            }

            #endregion

            #region fields

            private ObservableDictionary<TKey, TValue> dictionary;
            private int version;
            private int index;
            private KeyValuePair<TKey, TValue> current;
            private bool isDictionaryEntryEnumerator;

            #endregion fields
        }

        #endregion Enumerator

        #endregion public structures

        #region fields

        private int countCache;
        private Dictionary<TKey, TValue> dictionaryCache = new Dictionary<TKey, TValue>();
        private int dictionaryCacheVersion;
        protected KeyedDictionaryEntryCollection keyedEntryCollection;

        [NonSerialized]
        private SerializationInfo siInfo;

        private int version;

        #endregion fields
    }
}