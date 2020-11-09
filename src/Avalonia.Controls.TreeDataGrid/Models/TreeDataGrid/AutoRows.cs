using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    public class AutoRows : IRows
    {
        public IRow this[int index] => AutoRow.Instance;

        public int Count { get; set; }

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public IEnumerator<IRow> GetEnumerator()
        {
            for (var i = 0; i < Count; ++i)
                yield return AutoRow.Instance;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private class AutoRow : IRow
        {
            public static readonly AutoRow Instance = new AutoRow();
            public object? Header => null;
            public GridLength Height 
            { 
                get => GridLength.Auto; 
                set => throw new NotSupportedException(); 
            }
        }
    }
}
