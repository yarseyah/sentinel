namespace Sentinel.Interfaces
{
    using System;
    using System.Collections.Generic;

    public class CaseInsensitiveComparer<T> : IEqualityComparer<T>
    {
        public bool Equals(T x, T y)
        {
            if (x is string)
            {
                return string.Compare(x as string, y as string, StringComparison.OrdinalIgnoreCase) == 0;
            }

            return x.Equals(y);
        }

        public int GetHashCode(T obj)
        {
            throw new NotImplementedException();
        }
    }
}