using System;
using System.Collections.Generic;
using System.Text;

namespace Avalonia.Controls.Utils
{
    public static class Quicksort
    {
        public static int[] SortToMap<T>(IReadOnlyList<T> items, IComparer<T> comparer)
        {
            int[] map = new int[items.Count];
            
            for (int i = 0; i < map.Length; i++)
            {
                map[i] = i;
            }

            return map;
        }
    }
}
