using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Controls.Selection;
using Avalonia.Utilities;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Exposes a sortable collection of models as anonymous rows.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    /// <remarks>
    /// In a flat grid where rows cannot be resized, it is not necessary to persist any information
    /// about rows; the same row object can be updated and reused when a new row is requested.
    /// </remarks>
    public class AnonymousSortableRows<TModel> : ReadOnlyListBase<IRow<TModel>>, IRows, IDisposable
    {
        private readonly AnonymousRow<TModel> _row;
        private ItemsSourceViewFix<TModel> _items;
        private IComparer<TModel>? _comparer;
        private List<TModel>? _sortedItems;

        public AnonymousSortableRows(
            ItemsSourceViewFix<TModel> items,
            IComparer<TModel>? comparer)
        {
            _items = items;
            _items.CollectionChanged += OnItemsCollectionChanged;
            _comparer = comparer;
            _row = new AnonymousRow<TModel>();
        }

        public override IRow<TModel> this[int index]
        {
            get
            {
                if (_comparer is null)
                    return _row.Update(index, _items[index]);

                _sortedItems ??= CreateSortedItems(_comparer);
                return _row.Update(index, _sortedItems[index]);
            }
        }

        IRow IReadOnlyList<IRow>.this[int index] => this[index];
        public override int Count => _sortedItems?.Count ?? _items.Count;

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public void Dispose() => SetItems(ItemsSourceViewFix<TModel>.Empty);

        public (int index, double y) GetRowAt(double y)
        {
            // Rows in an AnonymousSortableRows collection have Auto height so we only
            // know the start position of the first row.
            if (MathUtilities.IsZero(y))
                return (0, 0);
            return (-1, -1);
        }

        public override IEnumerator<IRow<TModel>> GetEnumerator()
        {
            for (var i = 0; i < Count; ++i)
                yield return this[i];
        }

        public ICell RealizeCell(IColumn column, int columnIndex, int rowIndex)
        {
            if (column is IColumn<TModel> c)
                return c.CreateCell(this[rowIndex]);
            else
                throw new InvalidOperationException("Invalid column.");
        }

        public void SetItems(ItemsSourceViewFix<TModel> items)
        {
            _items.CollectionChanged -= OnItemsCollectionChanged;
            _items = items;

            if (!ReferenceEquals(items, ItemsSourceViewFix<TModel>.Empty))
                _items.CollectionChanged += OnItemsCollectionChanged;

            OnItemsCollectionChanged(null, CollectionExtensions.ResetEvent);
        }

        public void Sort(IComparer<TModel>? comparer, ISelectionModel selection)
        {
            _comparer = comparer;

            //if (_comparer is null && _sortedItems is object)
            //    _sortedItems = null;
            //else
            //    _sortedItems ??= new List<TModel>();

            _sortedItems = _items.OrderByWithSelectionPreserving(x => x, comparer,selection).ToList();

           // OnItemsCollectionChanged(null, CollectionExtensions.ResetEvent);
        }

        public void UnrealizeCell(ICell cell, int columnIndex, int rowIndex)
        {
            (cell as IDisposable)?.Dispose();
        }

        IEnumerator<IRow> IEnumerable<IRow>.GetEnumerator() => GetEnumerator();

        private List<TModel> CreateSortedItems(IComparer<TModel> comparer)
        {
            return _items.OrderBy(x => x, comparer).ToList();
        }

        private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (_comparer is null)
                OnItemsCollectionChangedUnsorted(e);
            else
                OnItemsCollectionChangedSorted(e);
        }

        private void OnItemsCollectionChangedUnsorted(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged is null)
                return;
            
            var ev = e.Action switch
            {
                NotifyCollectionChangedAction.Add => new NotifyCollectionChangedEventArgs(
                    e.Action,
                    new AnonymousRowItems<TModel>(e.NewItems),
                    e.NewStartingIndex),
                NotifyCollectionChangedAction.Remove => new NotifyCollectionChangedEventArgs(
                    e.Action,
                    new AnonymousRowItems<TModel>(e.OldItems),
                    e.OldStartingIndex),
                NotifyCollectionChangedAction.Replace => new NotifyCollectionChangedEventArgs(
                    e.Action,
                    new AnonymousRowItems<TModel>(e.NewItems),
                    new AnonymousRowItems<TModel>(e.OldItems),
                    e.OldStartingIndex),
                NotifyCollectionChangedAction.Move => new NotifyCollectionChangedEventArgs(
                    e.Action,
                    new AnonymousRowItems<TModel>(e.NewItems),
                    e.NewStartingIndex,
                    e.OldStartingIndex),
                NotifyCollectionChangedAction.Reset => e,
                _ => throw new NotSupportedException(),
            };
            
            CollectionChanged(this, ev);
        }

        private void OnItemsCollectionChangedSorted(NotifyCollectionChangedEventArgs e)
        {
            // If the rows have not yet been read then the type of collection change shouldn't be
            // important; the only thing we need to do is inform the presenter that the collection
            // has changed so that it can display the new items if the previous items were empty.
            if (_sortedItems is null)
            {
                CollectionChanged?.Invoke(this, CollectionExtensions.ResetEvent);
                return;
            }

            void Add(IList items)
            {
                foreach (TModel model in items)
                {
                    var index = _sortedItems.BinarySearch(model, _comparer);
                    if (index < 0)
                        index = ~index;
                    _sortedItems.Insert(index, model);
                    CollectionChanged?.Invoke(
                        this,
                        new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Add,
                            _row.Update(index, model),
                            index));
                }
            }

            void Remove(IList items)
            {
                foreach (TModel model in items)
                {
                    var index = _sortedItems.BinarySearch(model, _comparer);

                    if (index >= 0)
                    {
                        _sortedItems.RemoveAt(index);
                        CollectionChanged?.Invoke(
                            this,
                            new NotifyCollectionChangedEventArgs(
                                NotifyCollectionChangedAction.Remove,
                                _row.Update(index, model),
                                index));
                    }
                }
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Add(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Remove(e.OldItems);
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                    Remove(e.OldItems);
                    Add(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _sortedItems = CreateSortedItems(_comparer!);
                    CollectionChanged?.Invoke(this, e);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
