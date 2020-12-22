using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia.Controls.Models.TreeDataGrid;

namespace Avalonia.Controls
{
    public class FlatTreeDataGridSource<TModel> : ITreeDataGridSource
    {
        private readonly ItemsSourceView<TModel> _items;
        private readonly ColumnList<TModel> _columns;
        private SortableRows<TModel>? _rows;
        private CellList? _cells;
        private IComparer<TModel>? _comparer;

        public FlatTreeDataGridSource(IEnumerable<TModel> items)
        {
            _items = new ItemsSourceView<TModel>(items);
            _columns = new ColumnList<TModel>();
        }

        public IColumns Columns => _columns;
        public IRows Rows => _rows ??= InitializeRows();
        public ICells Cells => _cells ??= InitializeCells();

        public void AddColumn<TValue>(
            string header,
            Func<TModel, TValue> selector,
            GridLength? width = null)
        {
            var columnWidth = width ?? new GridLength(1, GridUnitType.Star);
            var column = new TextColumn<TModel, TValue>(header, selector, columnWidth);
            _columns.Add(column);
        }

        public void AddColumn(ColumnBase<TModel> column) => _columns.Add(column);

        public void SetSort(Func<TModel, TModel, int>? comparer)
        {
            _comparer = comparer is object ? new Comparer(comparer) : null;
            _rows?.SetSort(_comparer);
        }

        public bool SortBy(ColumnBase<TModel> column, ListSortDirection direction)
        {
            if (!_columns.Contains(column))
                return false;

            var comparer = column.GetComparer(direction);

            if (comparer is object)
            {
                SetSort(comparer);
                foreach (var c in _columns)
                    c.SortDirection = c == column ? (ListSortDirection?)direction : null;
                return true;
            }

            return false;
        }

        bool ITreeDataGridSource.SortBy(IColumn? column, ListSortDirection direction)
        {
            if (column is ColumnBase<TModel> typedColumn)
            {
                SortBy(typedColumn, direction);
                return true;
            }

            return false;
        }

        private SortableRows<TModel> InitializeRows()
        {
            var result = new SortableRows<TModel>(_items, _comparer);
            result.CollectionChanged += RowsCollectionChanged;
            return result;
        }

        private CellList InitializeCells()
        {
            var result = new CellList(_columns.Count);
            Reset(result);
            return result;
        }

        private ICell CreateCell(TModel model, int columnIndex)
        {
            return _columns[columnIndex] switch
            {
                SelectorColumnBase<TModel> column => column.CreateCell(model),
                _ => throw new NotSupportedException("Unsupported column type"),
            };
        }

        private void Reset(CellList cells)
        {
            _rows ??= InitializeRows();
            cells.Clear();

            foreach (var row in _rows)
            {
                var columnCount = _columns.Count;

                for (var columnIndex = 0; columnIndex < columnCount; ++columnIndex)
                    cells.Add(CreateCell(row.Model, columnIndex));
            }
        }

        private void RowsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_cells is null)
                return;

            void Add(int rowIndex, IList rows)
            {
                var cellIndex = rowIndex * _columns.Count;
                var columnCount = _columns.Count;

                foreach (RowBase<TModel> row in rows)
                {
                    for (var columnIndex = 0; columnIndex < columnCount; ++columnIndex)
                        _cells.Insert(cellIndex++, CreateCell(row.Model, columnIndex));
                }
            }

            void Remove(int rowIndex, int rowCount)
            {
                _cells.RemoveRange(rowIndex * _columns.Count, rowCount * _columns.Count);
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Add(e.NewStartingIndex, e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Remove(e.OldStartingIndex, e.OldItems.Count);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    var cellIndex = e.NewStartingIndex * _columns.Count;
                    var columnCount = _columns.Count;

                    foreach (RowBase<TModel> row in e.NewItems)
                    {
                        for (var columnIndex = 0; columnIndex < columnCount; ++columnIndex)
                            _cells[cellIndex++] = CreateCell(row.Model, columnIndex);
                    }

                    break;
                case NotifyCollectionChangedAction.Move:
                    Remove(e.OldStartingIndex, e.OldItems.Count);
                    Add(e.NewStartingIndex, e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Reset(_cells);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private class Comparer : IComparer<TModel>
        {
            private readonly Func<TModel, TModel, int> _func;
            public Comparer(Func<TModel, TModel, int> func) => _func = func;
            public int Compare(TModel x, TModel y) => _func(x, y);
        }
    }
}
