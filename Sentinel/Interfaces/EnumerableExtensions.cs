namespace Sentinel.Interfaces
{
    using System.Collections.Generic;
    using System.Linq;

    public static class EnumerableExtensions
    {
        public static int IndexOf<T>(this IEnumerable<T> list, T value)
        {
            return list.IndexOf(value, null);
        }

        public static int IndexOf<T>(this IEnumerable<T> list, T value, IEqualityComparer<T> comparer)
        {
            comparer = comparer ?? EqualityComparer<T>.Default;
            var found = list.Select((a, i) => new { a, i }).FirstOrDefault(x => comparer.Equals(x.a, value));
            return found?.i ?? -1;
        }
    }
}