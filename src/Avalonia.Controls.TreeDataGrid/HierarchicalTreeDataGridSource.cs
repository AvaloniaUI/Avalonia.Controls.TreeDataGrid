using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia.Controls.Models.TreeDataGrid;

namespace Avalonia.Controls
{
    /// <summary>
    /// A data source for a <see cref="TreeDataGrid"/> which displays a hierarchial tree where each
    /// row may have multiple columns.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public class HierarchicalTreeDataGridSource<TModel> : ITreeDataGridSource
        where TModel : class
    {
        private readonly ItemsSourceView<TModel> _roots;
        private readonly ColumnList<TModel> _columns;
        private IExpanderColumn<TModel>? _expanderColumn;
        private HierarchicalRows<TModel>? _rows;
        private CellList? _cells;
        private Comparison<TModel>? _comparison;

        public HierarchicalTreeDataGridSource(TModel root)
            : this(new[] { root })
        {
        }

        public HierarchicalTreeDataGridSource(IEnumerable<TModel> roots)
        {
            _roots = ItemsSourceView<TModel>.GetOrCreate(roots);
            _columns = new ColumnList<TModel>();
        }

        public ICells Cells => _cells ??= CreateCells();
        public IRows Rows => _rows ??= CreateRows();
        public IColumns Columns => _columns;

        public void AddExpanderColumn<TValue>(
            string header,
            Func<TModel, TValue> valueSelector,
            Func<TModel, IEnumerable<TModel>?> childSelector,
            Func<TModel, bool>? hasChildrenSelector = null,
            GridLength? width = null)
        {
            var columnWidth = width ?? new GridLength(1, GridUnitType.Star);
            var column = new HierarchicalExpanderColumn<TModel, TValue>(
                header,
                valueSelector,
                childSelector,
                hasChildrenSelector,
                columnWidth);
            AddColumn(column);
        }

        public void AddColumn<TValue>(
            string header,
            Func<TModel, TValue> selector,
            GridLength? width = null)
        {
            var columnWidth = width ?? new GridLength(1, GridUnitType.Star);
            var column = new TextColumn<TModel, TValue>(
                header,
                columnWidth,
                selector);
            AddColumn(column);
        }

        public void AddColumn(IColumn<TModel> column)
        {
            _columns.Add(column);
            _expanderColumn ??= column as IExpanderColumn<TModel>;
        }

        public void Sort(Comparison<TModel>? comparison)
        {
            _comparison = comparison;
            _rows?.Sort(_comparison);
        }

        bool ITreeDataGridSource.SortBy(IColumn? column, ListSortDirection direction)
        {
            if (column is IColumn<TModel> columnBase &&
                _columns.Contains(columnBase) &&
                columnBase.GetComparison(direction) is Comparison<TModel> comparison)
            {
                Sort(comparison);
                foreach (var c in _columns)
                    c.SortDirection = c == column ? (ListSortDirection?)direction : null;
                return true;
            }

            return false;
        }

        private HierarchicalRows<TModel> CreateRows()
        {
            if (_columns.Count == 0)
                throw new InvalidOperationException("No columns defined.");
            if (_expanderColumn is null)
                throw new InvalidOperationException("No expander column defined.");

            var result = new HierarchicalRows<TModel>(_roots, _expanderColumn, _comparison);
            result.CollectionChanged += RowsCollectionChanged;
            return result;
        }

        private CellList CreateCells()
        {
            var result = new CellList(_columns.Count);
            Reset(result);
            return result;
        }

        private ICell CreateCell(HierarchicalRow<TModel> row, int columnIndex)
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

                foreach (HierarchicalRow<TModel> row in rows)
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

                    foreach (HierarchicalRow<TModel> row in e.NewItems)
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