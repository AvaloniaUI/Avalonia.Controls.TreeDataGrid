using System;
using System.Collections.Generic;
using Avalonia.Controls.Models.TreeDataGrid;

namespace Avalonia.Controls
{
    public class FlatTreeDataGridSource<TModel> : ITreeDataGridSource
    {
        private readonly ItemsSourceView<TModel> _items;
        private readonly ColumnList<TModel> _columns;
        private readonly AutoRows _rows;
        private CellList? _cells;

        public FlatTreeDataGridSource(IEnumerable<TModel> items)
        {
            _items = new ItemsSourceView<TModel>(items);
            _columns = new ColumnList<TModel>();
            _rows = new AutoRows();
        }

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

        public IColumns Columns => _columns;
        public IRows Rows => _rows;
        public ICells Cells => _cells ??= InitializeCells();

        private CellList InitializeCells()
        {
            var result = new CellList(_columns.Count);

            if (_columns is object)
            {
                _rows.Count = 0;
                InsertRows(result, 0, _items);
            }

            return result;
        }

        private int InsertRows(
            CellList cells,
            int rowIndex,
            IEnumerable<TModel> models)
        {
            var childRowIndex = 0;

            foreach (var row in models)
            {
                cells.InsertRow(rowIndex++, row, (model, columnIndex) => _columns[columnIndex] switch
                {
                    StandardColumnBase<TModel> column => column.CreateCell(model),
                    _ => throw new NotSupportedException("Unsupported column type"),
                });

                ++_rows.Count;
            }

            return childRowIndex;
        }

    }
}
