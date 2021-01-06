using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    internal static class CollectionExtensions
    {
        public static readonly NotifyCollectionChangedEventArgs ResetEvent =
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);

        public static int BinarySearch<TModel>(
            this IReadOnlyList<RowBase<TModel>> items,
            TModel model,
            Comparison<TModel> comparison,
            int from = 0,
            int to = -1)
        {
            to = to == -1 ? items.Count - 1 : to;

            var lo = from;
            var hi = to;

            while (lo <= hi)
            {
                // PERF: `lo` or `hi` will never be negative inside the loop,
                //       so computing median using uints is safe since we know
                //       `length <= int.MaxValue`, and indices are >= 0
                //       and thus cannot overflow an uint.
                //       Saves one subtraction per loop compared to
                //       `int i = lo + ((hi - lo) >> 1);`
                var i = (int)(((uint)hi + (uint)lo) >> 1);
                var c = comparison(model, items[i].Model);
                if (c == 0)
                    return i;
                else if (c > 0)
                    lo = i + 1;
                else
                    hi = i - 1;
            }

            // If none found, then a negative number that is the bitwise complement
            // of the index of the next element that is larger than or, if there is
            // no larger element, the bitwise complement of `length`, which
            // is `lo` at this point.
            return ~lo;
        }

        public static T[] Slice<T>(this List<T> list, int index, int count)
        {
            var result = new T[count];
            list.CopyTo(index, result, 0, count);
            return result;
        }
    }
}
