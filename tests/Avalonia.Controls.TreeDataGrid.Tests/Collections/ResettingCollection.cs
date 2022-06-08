using System.Collections.Generic;
using System.Collections.Specialized;

namespace Avalonia.Controls.TreeDataGridTests.Collections
{
    internal class ResettingCollection<T> : List<T>, INotifyCollectionChanged
    {
        public ResettingCollection(IEnumerable<T> items)
        {
            AddRange(items);
        }

        public new void RemoveAt(int index)
        {
            var item = this[index];
            base.RemoveAt(index);
            CollectionChanged?.Invoke(
                this,
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove,
                    item,
                    index));
        }

        public void Reset(IEnumerable<T> items)
        {
            Clear();
            AddRange(items);
            CollectionChanged?.Invoke(
                this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public event NotifyCollectionChangedEventHandler? CollectionChanged;
    }
}
