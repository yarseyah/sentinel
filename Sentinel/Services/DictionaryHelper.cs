using System;
using System.Collections.Generic;

namespace Sentinel.Services
{
    public static class DictionaryHelper
    {
        /// <summary>
        /// Safely returns the value from the dictionary associated to the key supplied, or the default (usually a null) for the 
        /// value type.
        /// </summary>
        /// <typeparam name="TKey">Dictionary key type, can usually be deduced from the <see cref="dict"/> parameter.</typeparam>
        /// <typeparam name="TValue">Dictionary value type, can usually be deduced from the <see cref="dict"/> parameter.</typeparam>
        /// <param name="dict">Dictionary in which to look.</param>
        /// <param name="key">Key to find in dictionary.</param>
        /// <returns>Value corresponding to the supplied key.</returns>
        public static TValue Get<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
        {
            if (dict == null)
            {
                throw new ArgumentNullException("dict");
            }

            return dict.ContainsKey(key) ? dict[key] : default(TValue);
        }
    }
}