using System;
using System.Collections;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    internal class RowItems<TModel> : IList
    {
        private readonly IList _items;
        private readonly AnonymousRow<TModel> _row;

        public RowItems(IList items)
        {
            _items = items;
            _row = new AnonymousRow<TModel>();
        }

        public object? this[int index] 
        {
            get => _row.Update(index, (TModel)_items[index]);
            set => throw new NotSupportedException(); 
        }

        bool IList.IsFixedSize => true;
        bool IList.IsReadOnly => true;
        int ICollection.Count => _items.Count;
        bool ICollection.IsSynchronized => false;
        object? ICollection.SyncRoot => null;

        public IEnumerator GetEnumerator()
        {
            var count = _items.Count;
            for (var i = 0; i < count; ++i)
                yield return this[i];
        }

        int IList.Add(object value) => throw new NotSupportedException();
        void IList.Clear() => throw new NotSupportedException();
        bool IList.Contains(object value) => throw new NotSupportedException();
        void ICollection.CopyTo(Array array, int index) => throw new NotSupportedException();
        int IList.IndexOf(object value) => throw new NotSupportedException();
        void IList.Insert(int index, object value) => throw new NotSupportedException();
        void IList.Remove(object value) => throw new NotSupportedException();
        void IList.RemoveAt(int index) => throw new NotSupportedException();
    }
}
