using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia.Controls.Models.TreeDataGrid;

namespace Avalonia.Controls
{
    /// <summary>
    /// A data source for a <see cref="TreeDataGrid"/> which displays a flat grid.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public class FlatTreeDataGridSource<TModel> : ITreeDataGridSource
    {
        private readonly ItemsSourceView<TModel> _items;
        private readonly ColumnList<TModel> _columns;
        private AnonymousSortableRows<TModel>? _rows;
        private CellList? _cells;
        private IComparer<TModel>? _comparer;

        public FlatTreeDataGridSource(IEnumerable<TModel> items)
        {
            _items = ItemsSourceView<TModel>.GetOrCreate(items);
            _columns = new ColumnList<TModel>();
        }

        public IColumns Columns => _columns;
        public IRows Rows => _rows ??= CreateRows();
        public ICells Cells => _cells ??= CreateCells();

        public void AddColumn<TValue>(
            string header,
            Func<TModel, TValue> selector,
            GridLength? width = null)
        {
            var columnWidth = width ?? new GridLength(1, GridUnitType.Star);
            var column = new TextColumn<TModel, TValue>(header, columnWidth, selector);
            _columns.Add(column);
        }

        public void AddColumn(IColumn<TModel> column) => _columns.Add(column);

        public void Sort(Comparison<TModel>? comparer)
        {
            _comparer = comparer is object ? new FuncComparer<TModel>(comparer) : null;
            _rows?.Sort(_comparer);
        }

        public bool SortBy(IColumn<TModel> column, ListSortDirection direction)
        {
            if (!_columns.Contains(column))
                return false;

            var comparer = column.GetComparison(direction);

            if (comparer is object)
            {
                Sort(comparer);
                foreach (var c in _columns)
                    c.SortDirection = c == column ? (ListSortDirection?)direction : null;
                return true;
            }

            return false;
        }

        bool ITreeDataGridSource.SortBy(IColumn? column, ListSortDirection direction)
        {
            if (column is IColumn<TModel> typedColumn)
            {
                SortBy(typedColumn, direction);
                return true;
            }

            return false;
        }

        private AnonymousSortableRows<TModel> CreateRows()
        {
            var result = new AnonymousSortableRows<TModel>(_items, _comparer);
            result.CollectionChanged += RowsCollectionChanged;
            return result;
        }

        private CellList CreateCells()
        {
            var result = new CellList(_columns.Count);
            Reset(result);
            return result;
        }

        private ICell CreateCell(IRow<TModel> row, int columnIndex)
        {
            return _columns[columnIndex].CreateCell(row);
        }

        private void Reset(CellList cells)
        {
            _rows ??= CreateRows();
            cells.Clear();

            foreach (var row in _rows)
            {
                var columnCount = _columns.Count;

                for (var columnIndex = 0; columnIndex < columnCount; ++columnIndex)
                    cells.Add(CreateCell(row, columnIndex));
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

                foreach (IRow<TModel> row in rows)
                {
                    for (var columnIndex = 0; columnIndex < columnCount; ++columnIndex)
                        _cells.Insert(cellIndex++, CreateCell(row, columnIndex));
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

                    foreach (IRow<TModel> row in e.NewItems)
                    {
                        for (var columnIndex = 0; columnIndex < columnCount; ++columnIndex)
                            _cells[cellIndex++] = CreateCell(row, columnIndex);
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
    }
}
