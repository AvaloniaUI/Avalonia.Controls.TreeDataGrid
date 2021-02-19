using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    public class HierarchicalRows<TModel> : ReadOnlyListBase<HierarchicalRow<TModel>>,
        IRows,
        IDisposable,
        IExpanderRowController<TModel>
    {
        private readonly IExpanderRowController<TModel> _controller;
        private readonly RootRows _roots;
        private readonly IExpanderColumn<TModel> _expanderColumn;
        private readonly List<HierarchicalRow<TModel>> _rows;
        private Comparison<TModel>? _comparison;

        public HierarchicalRows(
            IExpanderRowController<TModel> controller,
            ItemsSourceViewFix<TModel> items,
            IExpanderColumn<TModel> expanderColumn,
            Comparison<TModel>? comparison)
        {
            _controller = controller;
            _roots = new RootRows(this, items, comparison);
            _roots.CollectionChanged += OnRootsCollectionChanged;
            _expanderColumn = expanderColumn;
            _comparison = comparison;
            _rows = new List<HierarchicalRow<TModel>>();
            InitializeRows();
        }

        public override HierarchicalRow<TModel> this[int index] => _rows[index];
        IRow IReadOnlyList<IRow>.this[int index] => _rows[index];
        public override int Count => _rows.Count;

        public void Dispose() => _roots.Dispose();

        public void SetItems(ItemsSourceViewFix<TModel> items)
        {
            _roots.SetItems(items);
        }

        public void Sort(Comparison<TModel>? comparison)
        {
            _comparison = comparison;
            _roots.Sort(comparison);
            _rows.Clear();
            InitializeRows();
            CollectionChanged?.Invoke(this, CollectionExtensions.ResetEvent);

            foreach (var row in _roots)
            {
                row.SortChildren(comparison);
            }
        }

        public override IEnumerator<HierarchicalRow<TModel>> GetEnumerator() => _rows.GetEnumerator();
        IEnumerator<IRow> IEnumerable<IRow>.GetEnumerator() => _rows.GetEnumerator();

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        void IExpanderRowController<TModel>.OnBeginExpandCollapse(IExpanderRow<TModel> row)
        {
            _controller.OnBeginExpandCollapse(row);
        }

        void IExpanderRowController<TModel>.OnEndExpandCollapse(IExpanderRow<TModel> row)
        {
            _controller.OnEndExpandCollapse(row);
        }

        void IExpanderRowController<TModel>.OnChildCollectionChanged(
            IExpanderRow<TModel> row,
            NotifyCollectionChangedEventArgs e)
        {
            if (row is HierarchicalRow<TModel> h)
                OnCollectionChanged(h.ModelIndexPath, e);
            else
                throw new NotSupportedException("Unexpected row type.");
        }

        internal int GetRowIndex(in IndexPath index, int fromRowIndex = 0)
        {
            if (index.GetSize() > 0)
            {
                for (var i = fromRowIndex; i < _rows.Count; ++i)
                {
                    if (index == _rows[i].ModelIndexPath)
                        return i;
                }
            }

            return -1;
        }

        private void InitializeRows()
        {
            var i = 0;

            foreach (var model in _roots)
            {
                i += AddRowsAndDescendants(i, model);
            }
        }

        private int AddRowsAndDescendants(int index, HierarchicalRow<TModel> row)
        {
            var i = index;
            _rows.Insert(i++, row);

            if (row.Children is object)
            {
                foreach (var childRow in row.Children)
                {
                    i += AddRowsAndDescendants(i, childRow);
                }
            }

            return i - index;
        }

        private void OnCollectionChanged(in IndexPath parentIndex, NotifyCollectionChangedEventArgs e)
        {
            void Add(int index, IEnumerable? items, bool raise)
            {
                if (items is null)
                    return;

                var start = index;

                foreach (HierarchicalRow<TModel> row in items)
                {
                    index += AddRowsAndDescendants(index, row);
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
                if (count == 0)
                    return;

                var oldItems = raise && CollectionChanged is object ?
                    new HierarchicalRow<TModel>[count] : null;

                for (var i = 0; i < count; ++i)
                {
                    var row = _rows[i + index];
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
                if (rowIndex == -1)
                    return _rows.Count;

                var row = _rows[rowIndex];
                var depth = row.ModelIndexPath.GetSize();
                var i = rowIndex + 1;

                while (i < _rows.Count && _rows[i].ModelIndexPath.GetSize() > depth)
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
                        var children = parentRowIndex >= 0 ? _rows[parentRowIndex].Children : _roots;
                        var count = GetDescendentRowCount(parentRowIndex);
                        Remove(parentRowIndex + 1, count, true);
                        Add(parentRowIndex + 1, children, true);
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
            IReadOnlyList<HierarchicalRow<TModel>>
        {
            private readonly HierarchicalRows<TModel> _owner;

            public RootRows(
                HierarchicalRows<TModel> owner,
                ItemsSourceViewFix<TModel> items,
                Comparison<TModel>? comparison)
                : base(items, comparison)
            {
                _owner = owner;
            }

            protected override HierarchicalRow<TModel> CreateRow(int modelIndex, TModel model)
            {
                return new HierarchicalRow<TModel>(
                    _owner,
                    _owner._expanderColumn,
                    new IndexPath(modelIndex),
                    model,
                    _owner._comparison);
            }
        }
    }
}
