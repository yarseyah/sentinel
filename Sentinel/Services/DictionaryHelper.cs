namespace Sentinel.Services
{
    using System;
    using System.Collections.Generic;

    using Sentinel.Interfaces;
    using Sentinel.Interfaces.CodeContracts;

    public static class DictionaryHelper
    {
        /// <summary>
        /// Safely returns the value from the dictionary associated to the key supplied, or the default (usually a null) for the
        /// value type.
        /// </summary>
        /// <typeparam name="TKey">Dictionary key type, can usually be deduced from the dictionary parameter.</typeparam>
        /// <typeparam name="TValue">Dictionary value type, can usually be deduced from the dictionary> parameter.</typeparam>
        /// <param name="dictionary">Dictionary in which to look.</param>
        /// <param name="key">Key to find in dictionary.</param>
        /// <returns>Value corresponding to the supplied key.</returns>
        public static TValue Get<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            dictionary.ThrowIfNull(nameof(dictionary));
            return dictionary.ContainsKey(key) ? dictionary[key] : default(TValue);
        }
    }
}