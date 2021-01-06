using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    public class AnonymousSortableRows<TModel> : IRows, IEnumerable<RowBase<TModel>>
    {
        private readonly ItemsSourceView<TModel> _items;
        private readonly AnonymousRow<TModel> _row;
        private IComparer<TModel>? _comparer;
        private List<TModel>? _sortedItems;

        public AnonymousSortableRows(ItemsSourceView<TModel> items, IComparer<TModel>? comparer)
        {
            _items = items;
            _items.CollectionChanged += OnItemsCollectionChanged;
            _comparer = comparer;
            _row = new AnonymousRow<TModel>();
        }

        public RowBase<TModel> this[int index]
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
        public int Count => _sortedItems?.Count ?? _items.Count;

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public IEnumerator<RowBase<TModel>> GetEnumerator()
        {
            for (var i = 0; i < Count; ++i)
                yield return this[i];
        }

        public void SetSort(IComparer<TModel>? comparer)
        {
            _comparer = comparer;

            if (_comparer is null && _sortedItems is object)
                _sortedItems = null;
            else
                _sortedItems ??= new List<TModel>();

            OnItemsCollectionChanged(_items,  CollectionExtensions.ResetEvent);
        }

        IEnumerator<IRow> IEnumerable<IRow>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private List<TModel> CreateSortedItems(IComparer<TModel> comparer)
        {
            return _items.OrderBy(x => x, comparer).ToList();
        }

        private void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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
            if (_sortedItems is null)
                return;

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
