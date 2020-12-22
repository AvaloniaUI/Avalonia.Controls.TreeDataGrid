using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls.Models.TreeDataGrid;

namespace Avalonia.Controls
{
    public class HierarchicalTreeDataGridSource<TModel> : ITreeDataGridSource, IExpanderController
        where TModel : class
    {
        private readonly ItemsSourceView<TModel> _roots;
        private readonly ColumnList<TModel> _columns;
        private readonly AutoRows _rows;
        private readonly Func<TModel, IEnumerable<TModel>?> _childSelector;
        private readonly Func<TModel, bool> _hasChildrenSelector;
        private CellList? _cells;

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
            _rows = new AutoRows();
            _childSelector = childSelector;
            _hasChildrenSelector = hasChildrenSelector;
        }

        public ICells Cells => _cells ??= InitializeCells();
        public IColumns Columns => _columns;
        public IRows Rows => _rows;

        public void AddColumn<TValue>(
            string header,
            Func<TModel, TValue> selector,
            GridLength? width = null)
        {
            var columnWidth = width ?? new GridLength(1, GridUnitType.Star);
            var column = _columns.Count == 0 ?
                (ColumnBase<TModel>)new ExpanderColumn<TModel, TValue>(
                    this,
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

        bool IExpanderController.TryExpand(IExpander expander)
        {
            if (_cells is null)
                return false;

            if (expander is ExpanderCellBase<TModel> cell &&
                _columns.IndexOf(cell.Column) is var columnIndex &&
                columnIndex != -1 &&
                FindRowIndexForModelIndex(columnIndex, cell.ModelIndex) is var rowIndex &&
                rowIndex != -1 &&
                cell.GetChildModels() is IEnumerable<TModel> childModels)
            {
                return InsertRows(_cells, rowIndex + 1, cell.ModelIndex, childModels) > 0;
            }

            return false;
        }

        void IExpanderController.Collapse(IExpander expander)
        {
            if (_cells is null)
                return;

            if (expander is ExpanderCellBase<TModel> cell &&
                _columns.IndexOf(cell.Column) is var columnIndex &&
                columnIndex != -1 &&
                FindRowIndexForModelIndex(columnIndex, cell.ModelIndex) is var rowIndex &&
                rowIndex != -1)
            {
                var childRowCount = GetDescendentRowCount(columnIndex, rowIndex);
                _cells.RemoveRows(++rowIndex, childRowCount);
            }
        }

        private CellList InitializeCells()
        {
            var result = new CellList(_columns.Count);

            if (_columns is object)
            {
                _rows.Count = 0;
                InsertRows(result, 0, default, _roots);
            }

            return result;
        }

        private int InsertRows(
            CellList cells,
            int rowIndex,
            IndexPath parentModelIndex,
            IEnumerable<TModel> models)
        {
            var childRowIndex = 0;

            foreach (var row in models)
            {
                var modelIndex = parentModelIndex.CloneWithChildIndex(childRowIndex++);

                cells.InsertRow(rowIndex++, row, (model, columnIndex) => _columns[columnIndex] switch
                {
                    ExpanderColumnBase<TModel> expander => expander.CreateCell(modelIndex, model),
                    SelectorColumnBase<TModel> column => column.CreateCell(model),
                    _ => throw new NotSupportedException("Unsupported column type"),
                });

                ++_rows.Count;
            }

            return childRowIndex;
        }

        private int FindRowIndexForModelIndex(int columnIndex, IndexPath modelIndex)
        {
            if (_cells is null)
                return -1;

            // TODO: Do this with an index or binary search or something.
            for (var i = 0; i < _cells.RowCount; ++i)
            {
                if (_cells[columnIndex, i] is IExpanderCell cell && cell.ModelIndex == modelIndex)
                {
                    return i;
                }
            }

            return -1;
        }

        private int GetDescendentRowCount(int columnIndex, int rowIndex)
        {
            if (_cells is null || (rowIndex + 1) >= _cells.RowCount)
                return -1;

            var parentCell = _cells[columnIndex, rowIndex] as IExpanderCell ??
                throw new ArgumentException("Cannot count children of non-expander cell.");

            var depth = parentCell.ModelIndex.GetSize();
            var i = 1;

            while (rowIndex + i < _cells.RowCount &&
                _cells[columnIndex, rowIndex + i] is IExpanderCell cell &&
                cell.ModelIndex.GetSize() > depth)
            {
                ++i;
            }

            return i - 1;
        }
    }
}