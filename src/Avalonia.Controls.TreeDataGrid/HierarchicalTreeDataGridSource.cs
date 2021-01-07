using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia.Controls.Models.TreeDataGrid;

namespace Avalonia.Controls
{
    public class HierarchicalTreeDataGridSource<TModel> : ITreeDataGridSource
        where TModel : class
    {
        private readonly ItemsSourceView<TModel> _roots;
        private readonly ColumnList<TModel> _columns;
        private readonly Func<TModel, IEnumerable<TModel>?> _childSelector;
        private readonly Func<TModel, bool> _hasChildrenSelector;
        private HierarchicalRows<TModel>? _rows;
        private CellList? _cells;
        private Comparison<TModel>? _comparison;

        public HierarchicalTreeDataGridSource(
            TModel root,
            Func<TModel, IEnumerable<TModel>?> childSelector,
            Func<TModel, bool> hasChildrenSelector)
            : this(new[] { root }, childSelector, hasChildrenSelector)
        {
        }

        public HierarchicalTreeDataGridSource(
            IEnumerable<TModel> roots,
            Func<TModel, IEnumerable<TModel>?> childSelector,
            Func<TModel, bool> hasChildrenSelector)
        {
            _roots = new ItemsSourceView<TModel>(roots);
            _columns = new ColumnList<TModel>();
            _childSelector = childSelector;
            _hasChildrenSelector = hasChildrenSelector;
        }

        public ICells Cells => _cells ??= CreateCells();
        public IRows Rows => _rows ??= CreateRows();
        public IColumns Columns => _columns;

        public void AddColumn<TValue>(
            string header,
            Func<TModel, TValue> selector,
            GridLength? width = null)
        {
            var columnWidth = width ?? new GridLength(1, GridUnitType.Star);
            var column = _columns.Count == 0 ?
                (ColumnBase<TModel>)new ExpanderColumn<TModel, TValue>(
                    header,
                    selector,
                    _childSelector,
                    columnWidth,
                    _hasChildrenSelector) :
                new TextColumn<TModel, TValue>(
                    header,
                    selector,
                    columnWidth);
            _columns.Add(column);
        }

        public void AddColumn(ColumnBase<TModel> column) => _columns.Add(column);

        public void Sort(Comparison<TModel>? comparison)
        {
            _comparison = comparison;
            _rows?.Sort(_comparison);
        }

        bool ITreeDataGridSource.SortBy(IColumn? column, ListSortDirection direction) => false;

        private HierarchicalRows<TModel> CreateRows()
        {
            var result = new HierarchicalRows<TModel>(_roots, _comparison);
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
            return _columns[columnIndex] switch
            {
                ExpanderColumnBase<TModel> expander => expander.CreateCell(row),
                SelectorColumnBase<TModel> column => column.CreateCell(row.Model),
                _ => throw new NotSupportedException("Unsupported column type"),
            };
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