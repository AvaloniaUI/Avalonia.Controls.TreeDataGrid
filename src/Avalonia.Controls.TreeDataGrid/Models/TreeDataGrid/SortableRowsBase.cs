using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// An <see cref="IRows"/> collection which supports sorting.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    /// <typeparam name="TRow">The row type.</typeparam>
    public abstract class SortableRowsBase<TModel, TRow> : ReadOnlyListBase<TRow>, IDisposable
        where TRow : IModelRow<TModel>, IDisposable
    {
        private ItemsSourceViewFix<TModel> _items;
        private Comparison<TModel>? _comparison;
        private List<TRow>? _rows;

        public SortableRowsBase(ItemsSourceViewFix<TModel> items, Comparison<TModel>? comparison)
        {
            _items = items;
            _items.CollectionChanged += OnItemsCollectionChanged;
            _comparison = comparison;
        }

        public override TRow this[int index] => Rows[index];
        public override int Count => _rows?.Count ?? _items.Count;
        private List<TRow> Rows => _rows ??= CreateRows();

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public virtual void Dispose() => SetItems(ItemsSourceViewFix<TModel>.Empty);
        public override IEnumerator<TRow> GetEnumerator() => Rows.GetEnumerator();

        public void SetItems(ItemsSourceViewFix<TModel> items)
        {
            _items.CollectionChanged -= OnItemsCollectionChanged;
            _items = items;

            if (!ReferenceEquals(items, ItemsSourceViewFix<TModel>.Empty))
                _items.CollectionChanged += OnItemsCollectionChanged;

            OnItemsCollectionChanged(null, CollectionExtensions.ResetEvent);
        }

        public void Sort(Comparison<TModel>? comparison)
        {
            _comparison = comparison;

            if (_rows is object)
            {
                if (comparison is object)
                {
                    var comparer = new FuncComparer<TModel>(comparison);
                    _rows = _rows.OrderBy(x => x.Model, comparer).ToList();
                }
                else
                {
                    _rows.Sort((x, y) => x.ModelIndex.CompareTo(y.ModelIndex));
                }

                CollectionChanged?.Invoke(this, CollectionExtensions.ResetEvent);
            }
        }

        protected abstract TRow CreateRow(int modelIndex, TModel model);

        private List<TRow> CreateRows()
        {
            var result = new List<TRow>(_items.Count);
            Reset(result);
            return result;
        }

        private void Reset(List<TRow> rows)
        {
            foreach (var row in rows)
            {
                row.Dispose();
            }

            rows.Clear();

            if (_comparison is null)
            {
                for (var i = 0; i < _items.Count; ++i)
                {
                    rows.Add(CreateRow(i, _items[i]));
                }
            }
            else
            {
                var sorted = new (int index, TModel model)[_items.Count];
                var c = _comparison;

                for (var i = 0; i < _items.Count; ++i)
                {
                    sorted[i] = (i, _items[i]);
                }

                Array.Sort(sorted, (x, y) => c(x.model, y.model));

                foreach (var i in sorted)
                {
                    rows.Add(CreateRow(i.index, i.model));
                }
            }
        }

        private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
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

                while (index < _rows.Count)
                {
                    _rows[index++].UpdateModelIndex(items.Count);
                }
            }

            void Remove(int index, int count)
            {
                for (var i = index; i < index + count; ++i)
                {
                    _rows[i].Dispose();
                }

                _rows.RemoveRange(index, count);

                while (index < _rows.Count)
                {
                    _rows[index++].UpdateModelIndex(-count);
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
                        Remove(e.OldStartingIndex, e.OldItems.Count);
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
                    Remove(e.OldStartingIndex, e.OldItems.Count);
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
                foreach (var row in _rows)
                {
                    if (row.ModelIndex >= index)
                        row.UpdateModelIndex(items.Count);
                }

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

            void Remove(int index, IList items)
            {
                foreach (TModel model in items)
                {
                    var rowIndex = _rows.BinarySearch(model, _comparison!);

                    if (rowIndex >= 0)
                    {
                        var row = _rows[rowIndex];
                        row.Dispose();
                        _rows.RemoveAt(rowIndex);
                        CollectionChanged?.Invoke(
                            this,
                            new NotifyCollectionChangedEventArgs(
                                NotifyCollectionChangedAction.Remove,
                                row,
                                rowIndex));
                    }
                }

                foreach (var row in _rows)
                {
                    if (row.ModelIndex >= index)
                        row.UpdateModelIndex(-items.Count);
                }
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Add(e.NewStartingIndex, e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Remove(e.OldStartingIndex, e.OldItems);
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                    Remove(e.OldStartingIndex, e.OldItems);
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
