using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    public class HierarchicalRows<TModel> : IRows,
        IDisposable,
        IEnumerable<HierarchicalRow<TModel>>,
        IExpanderRowController<TModel>
    {
        private readonly RootRows _roots;
        private Comparison<TModel>? _comparison;
        private List<HierarchicalRow<TModel>> _rows;

        public HierarchicalRows(ItemsSourceView<TModel> roots, Comparison<TModel>? comparison)
        {
            _roots = new RootRows(this, roots, comparison);
            _roots.CollectionChanged += OnRootsCollectionChanged;
            _comparison = comparison;
            _rows = new List<HierarchicalRow<TModel>>(_roots);
        }

        public HierarchicalRow<TModel> this[int index] => _rows[index];
        IRow IReadOnlyList<IRow>.this[int index] => _rows[index];
        public int Count => _rows.Count;

        public void Dispose()
        {
            foreach (var row in _rows)
            {
                row.Dispose();
            }

            _roots.CollectionChanged -= OnRootsCollectionChanged;
        }

        public void Sort(Comparison<TModel>? comparison)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<HierarchicalRow<TModel>> GetEnumerator() => _rows.GetEnumerator();
        IEnumerator<IRow> IEnumerable<IRow>.GetEnumerator() => _rows.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _rows.GetEnumerator();

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        void IExpanderRowController<TModel>.OnChildCollectionChanged(ExpanderRowBase<TModel> row, NotifyCollectionChangedEventArgs e)
        {
            if (row is HierarchicalRow<TModel> h)
                OnCollectionChanged(h.ModelIndex, e);
            else
                throw new NotSupportedException("Unexpected row type.");
        }

        private int GetRowIndex(IndexPath index, int fromRowIndex = 0)
        {
            if (index.GetSize() > 0)
            {
                for (var i = fromRowIndex; i < _rows.Count; ++i)
                {
                    if (index == _rows[i].ModelIndex)
                        return i;
                }
            }

            return -1;
        }

        private void OnCollectionChanged(IndexPath parentIndex, NotifyCollectionChangedEventArgs e)
        {
            int AddRow(int index, HierarchicalRow<TModel> row)
            {
                var i = index;
                _rows.Insert(i++, row);

                if (row.Children is object)
                {
                    foreach (var childRow in row.Children)
                    {
                        i += AddRow(i, childRow);
                    }
                }

                return i - index;
            }

            void Add(int index, IEnumerable? items, bool raise)
            {
                if (items is null)
                    return;

                var start = index;

                foreach (HierarchicalRow<TModel> row in items)
                {
                    index += AddRow(index, row);
                }

                if (raise && index > start)
                {
                    CollectionChanged?.Invoke(
                        this,
                        new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Add,
                            new ListSpan(_rows, start, index - start),
                            start));
                }
            }

            void Remove(int index, int count, bool raise)
            {
                var oldItems = raise && CollectionChanged is object ?
                    new HierarchicalRow<TModel>[count] : null;

                for (var i = 0; i < count; ++i)
                {
                    var row = _rows[i + index];
                    row.Dispose();
                    if (oldItems is object)
                        oldItems[i] = row;
                }

                _rows.RemoveRange(index, count);
                
                if (oldItems is object)
                {
                    CollectionChanged!(
                        this,
                        new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Remove,
                            oldItems,
                            index));
                }
            }

            int Advance(int rowIndex, int count)
            {
                var i = rowIndex;

                while (count > 0)
                {
                    var row = _rows[i];
                    i += (row.Children?.Count ?? 0) + 1;
                    --count;
                }

                return i;
            }

            int GetDescendentRowCount(int rowIndex)
            {
                var row = _rows[rowIndex];
                var depth = row.ModelIndex.GetSize();
                var i = rowIndex + 1;

                while (i < _rows.Count && _rows[i].ModelIndex.GetSize() > depth)
                    ++i;

                return i - (rowIndex + 1);
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        var parent = GetRowIndex(parentIndex);
                        var insert = Advance(parent + 1, e.NewStartingIndex);
                        Add(insert, e.NewItems, true);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        var parent = GetRowIndex(parentIndex);
                        var start = Advance(parent + 1, e.OldStartingIndex);
                        var end = Advance(start, e.OldItems.Count);
                        Remove(start, end - start, true);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        var parent = GetRowIndex(parentIndex);
                        var start = Advance(parent + 1, e.OldStartingIndex);
                        var end = Advance(start, e.OldItems.Count);
                        Remove(start, end - start, true);
                        Add(start, e.NewItems, true);
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    {
                        var parent = GetRowIndex(parentIndex);
                        var fromStart = Advance(parent + 1, e.OldStartingIndex);
                        var fromEnd = Advance(fromStart, e.OldItems.Count);
                        var to = Advance(parent + 1, e.NewStartingIndex);
                        Remove(fromStart, fromEnd - fromStart, true);
                        Add(to, e.NewItems, true);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        var parentRowIndex = GetRowIndex(parentIndex);
                        var parentRow = _rows[parentRowIndex];
                        var count = GetDescendentRowCount(parentRowIndex);
                        Remove(parentRowIndex + 1, count, true);
                        Add(parentRowIndex + 1, parentRow.Children, true);
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private void OnRootsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnCollectionChanged(default, e);
        }

        private class RootRows : SortableRowsBase<TModel, HierarchicalRow<TModel>>,
            IEnumerable<HierarchicalRow<TModel>>
        {
            private readonly HierarchicalRows<TModel> _owner;

            public RootRows(
                HierarchicalRows<TModel> owner,
                ItemsSourceView<TModel> items,
                Comparison<TModel>? comparison)
                : base(items, comparison)
            {
                _owner = owner;
            }

            protected override HierarchicalRow<TModel> CreateRow(int modelIndex, TModel model)
            {
                return new HierarchicalRow<TModel>(_owner, new IndexPath(modelIndex), model, _owner._comparison);
            }
        }
    }
}
