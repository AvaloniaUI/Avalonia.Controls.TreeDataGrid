using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
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
            _items.CollectionChanged += OnItemsCollectionChanged;
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

        private void InsertRows(
            CellList cells,
            int rowIndex,
            IEnumerable<TModel> models)
        {
            foreach (var row in models)
                InsertRow(cells, rowIndex++, row);
        }

        private void InsertRow(CellList cells, int rowIndex, TModel row)
        {
            var columnCount = _columns.Count;
            var cellIndex = rowIndex * columnCount;

            for (var columnIndex = 0; columnIndex < columnCount; ++columnIndex)
                cells.Insert(cellIndex++, CreateCell(row, columnIndex));
            ++_rows.Count;
        }

        private ICell CreateCell(TModel model, int columnIndex)
        {
            return _columns[columnIndex] switch
            {
                SelectorColumnBase<TModel> column => column.CreateCell(model),
                _ => throw new NotSupportedException("Unsupported column type"),
            };
    }

        private void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_cells is null)
                return;

            void Add(int rowIndex, IEnumerable items)
            {
                foreach (var item in items)
                    if (item is TModel row)
                        InsertRow(_cells, rowIndex++, row);
            }

            void Remove(int rowIndex, int rowCount)
            {
                _cells.RemoveRange(rowIndex * _columns.Count, rowCount * _columns.Count);
                _rows.Count -= rowCount;
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
                    for (var i = 0; i < e.NewItems.Count; ++i)
                    {
                        for (var columnIndex = 0; columnIndex < _columns.Count; ++columnIndex)
                        {
                            if (e.NewItems[i] is TModel model)
                                _cells[columnIndex, e.NewStartingIndex + i] = CreateCell(model, columnIndex);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    Remove(e.OldStartingIndex, e.OldItems.Count);
                    Add(e.NewStartingIndex, e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _cells.Clear();
                    InsertRows(_cells, 0, _items);
                    _rows.Count = _items.Count;
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
