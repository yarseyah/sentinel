namespace Sentinel.Interfaces
{
    using System;
    using System.Collections.Generic;

    public class CaseInsensitiveComparer<T> : IEqualityComparer<T>
    {
        public bool Equals(T x, T y)
        {
            var stringX = x as string;

            if (stringX != null)
            {
                return string.Compare(stringX, y as string, StringComparison.OrdinalIgnoreCase) == 0;
            }

            return x.Equals(y);
        }

        public int GetHashCode(T obj)
        {
            throw new NotImplementedException();
        }
    }
}