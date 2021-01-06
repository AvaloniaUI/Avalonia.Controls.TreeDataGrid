using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    public abstract class SortableRowsBase<TModel, TRow> : IRows, IDisposable
        where TRow : RowBase<TModel>
    {
        private readonly ItemsSourceView<TModel> _items;
        private Comparison<TModel>? _comparison;
        private List<TRow>? _rows;

        public SortableRowsBase(ItemsSourceView<TModel> items, Comparison<TModel>? comparison)
        {
            _items = items;
            _items.CollectionChanged += OnItemsCollectionChanged;
            _comparison = comparison;
        }

        public TRow this[int index] => Rows[index];
        IRow IReadOnlyList<IRow>.this[int index] => Rows[index];
        public int Count => _rows?.Count ?? _items.Count;
        private List<TRow> Rows => _rows ??= CreateRows();

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public void Dispose() => _items.CollectionChanged -= OnItemsCollectionChanged;
        public IEnumerator<TRow> GetEnumerator() => Rows.GetEnumerator();

        IEnumerator<IRow> IEnumerable<IRow>.GetEnumerator()
        {
            for (var i = 0; i < Count; ++i)
                yield return this[i];
        }

        public void Sort(Comparison<TModel>? comparison)
        {
            _comparison = comparison;
            if (_rows is object)
                Reset(_rows);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        protected abstract TRow CreateRow(int modelIndex, TModel model);

        private List<TRow> CreateRows()
        {
            var result = new List<TRow>(_items.Count);
            Reset(result);
            return result;
        }

        private void Reset(List<TRow> rows)
        {
            rows.Clear();

            var items = (IReadOnlyList<TModel>)_items;

            if (_comparison is object)
            {
                var sorted = items.ToList();
                sorted.Sort(_comparison);
                items = sorted;
            }

            for (var i = 0; i < items.Count; ++i)
            {
                rows.Add(CreateRow(i, items[i]));
            }
        }

        private void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_comparison is null)
                OnItemsCollectionChangedUnsorted(e);
            else
                OnItemsCollectionChangedSorted(e);
        }

        private void OnItemsCollectionChangedUnsorted(NotifyCollectionChangedEventArgs e)
        {
            if (_rows is null)
                return;

            void Add(int index, IList items)
            {
                foreach (TModel model in items)
                {
                    _rows.Insert(index, CreateRow(index, model));
                    ++index;
                }
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Add(e.NewStartingIndex, e.NewItems);
                    CollectionChanged?.Invoke(
                        this,
                        new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Add,
                            new ListSpan(_rows, e.NewStartingIndex, e.NewItems.Count),
                            e.NewStartingIndex));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        var oldItems = CollectionChanged is object ?
                            _rows.Slice(e.OldStartingIndex, e.OldItems.Count) : null;
                        _rows.RemoveRange(e.OldStartingIndex, e.OldItems.Count);
                        CollectionChanged?.Invoke(
                            this,
                            new NotifyCollectionChangedEventArgs(
                                NotifyCollectionChangedAction.Remove,
                                oldItems,
                                e.OldStartingIndex));
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        var index = e.OldStartingIndex;
                        var count = e.OldItems.Count;
                        var oldItems = CollectionChanged is object ? _rows.Slice(index, count) : null;
                        
                        for (var i = 0; i < count; ++i)
                        {
                            _rows[index + i] = CreateRow(index + i, (TModel)e.NewItems[i]);
                        }

                        CollectionChanged?.Invoke(
                            this,
                            new NotifyCollectionChangedEventArgs(
                                NotifyCollectionChangedAction.Replace,
                                new ListSpan(_rows, index, count),
                                oldItems,
                                index));
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    _rows.RemoveRange(e.OldStartingIndex, e.OldItems.Count);
                    Add(e.NewStartingIndex, e.NewItems);
                    CollectionChanged?.Invoke(
                        this,
                        new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Move,
                            new ListSpan(_rows, e.NewStartingIndex, e.NewItems.Count),
                            e.NewStartingIndex,
                            e.OldStartingIndex));
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Reset(_rows);
                    CollectionChanged?.Invoke(this, e);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private void OnItemsCollectionChangedSorted(NotifyCollectionChangedEventArgs e)
        {
            if (_rows is null)
                return;

            void Add(int index, IList items)
            {
                foreach (TModel model in items)
                {
                    var rowIndex = _rows.BinarySearch(model, _comparison!);
                    var row = CreateRow(index, model);
                    if (rowIndex < 0)
                        rowIndex = ~rowIndex;
                    _rows.Insert(rowIndex, row);
                    CollectionChanged?.Invoke(
                        this,
                        new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Add,
                            row,
                            rowIndex));
                    ++rowIndex;
                }
            }

            void Remove(IList items)
            {
                foreach (TModel model in items)
                {
                    var index = _rows.BinarySearch(model, _comparison!);

                    if (index >= 0)
                    {
                        var row = _rows[index];
                        _rows.RemoveAt(index);
                        CollectionChanged?.Invoke(
                            this,
                            new NotifyCollectionChangedEventArgs(
                                NotifyCollectionChangedAction.Remove,
                                row,
                                index));
                    }
                }
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Add(e.NewStartingIndex, e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Remove(e.OldItems);
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                    Remove(e.OldItems);
                    Add(e.NewStartingIndex, e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Reset(_rows);
                    CollectionChanged?.Invoke(this, e);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
