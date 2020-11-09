using System;
using System.Collections.Generic;
using Avalonia.Controls.Models.TreeDataGrid;

namespace Avalonia.Controls
{
    public class HierarchicalTreeDataGridSource<TModel> : IExpanderController
        where TModel : class
    {
        private readonly List<TModel> _roots;
        private readonly ColumnList<TModel> _columns;
        private readonly AutoRows _rows;
        private readonly Func<TModel, IEnumerable<TModel>?> _childSelector;
        private readonly Func<TModel, bool> _hasChildrenSelector;
        private CellList? _cells;

        public HierarchicalTreeDataGridSource(
            TModel root,
            Func<TModel, IEnumerable<TModel>?> childSelector,
            Func<TModel, bool> hasChildrenSelector)
        {
            _roots = new List<TModel> { root };
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

        void IExpanderController.IsExpandedChanged(IExpander expander)
        {
            if (_cells is null)
                return;

            if (expander is ExpanderCellBase<TModel> cell &&
                _columns.IndexOf(cell.Column) is var columnIndex &&
                columnIndex != -1 &&
                FindRowIndexForModelIndex(columnIndex, cell.ModelIndex) is var index &&
                index != -1 &&
                cell.GetChildModels() is IEnumerable<TModel> childModels)
            {
                InsertRows(_cells, index + 1, cell.ModelIndex, childModels);
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

        private void InsertRows(
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
                    StandardColumnBase<TModel> column => column.CreateCell(model),
                    _ => throw new NotSupportedException("Unsupported column type"),
                });

                ++_rows.Count;
            }
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
    }
}