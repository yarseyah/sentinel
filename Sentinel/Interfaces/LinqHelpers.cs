namespace Sentinel.Interfaces
{
    using System.Collections.Generic;

    public static class LinqHelpers
    {
        public static IList<T> Swap<T>(this IList<T> list, int index1, int index2)
        {
            T temp = list[index1];
            list[index1] = list[index2];
            list[index2] = temp;
            return list;
        }
    }
}