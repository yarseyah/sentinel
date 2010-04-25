#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

using System;
using System.Collections.Generic;

#endregion

namespace Sentinel.Interfaces
{
    public class CaseInsensitiveComparer<T> : IEqualityComparer<T>
    {
        #region IEqualityComparer<T> Members

        public bool Equals(T x, T y)
        {
            if (x.GetType() == typeof(string))
            {
                return String.Compare(x as string, y as string, true) == 0;
            }

            return x.Equals(y);
        }

        public int GetHashCode(T obj)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}