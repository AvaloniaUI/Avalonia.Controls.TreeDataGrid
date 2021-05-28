using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Controls.Models.TreeDataGrid;

namespace Avalonia.Controls
{
    /// <summary>
    /// A data source for a <see cref="TreeDataGrid"/> which displays a flat grid.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public class FlatTreeDataGridSource<TModel> : ITreeDataGridSource, IDisposable
    {
        private IEnumerable<TModel> _items;
        private ItemsSourceViewFix<TModel> _itemsView;
        private AnonymousSortableRows<TModel>? _rows;
        private IComparer<TModel>? _comparer;

        public FlatTreeDataGridSource(IEnumerable<TModel> items)
        {
            _items = items;
            _itemsView = ItemsSourceViewFix<TModel>.GetOrCreate(items);
            Columns = new ColumnList<TModel>();
        }

        public ColumnList<TModel> Columns { get; }
        public IRows Rows => _rows ??= CreateRows();
        IColumns ITreeDataGridSource.Columns => Columns;

        public IEnumerable<TModel> Items
        {
            get => _items;
            set
            {
                if (_items != value)
                {
                    _items = value;
                    _itemsView = ItemsSourceViewFix<TModel>.GetOrCreate(value);
                    _rows?.SetItems(_itemsView);
                }
            }
        }

        public void Dispose() => _rows?.Dispose();

        public void Sort(Comparison<TModel>? comparer)
        {
            _comparer = comparer is object ? new FuncComparer<TModel>(comparer) : null;
            _rows?.Sort(_comparer);
        }

        public bool SortBy(IColumn<TModel> column, ListSortDirection direction)
        {
            if (!Columns.Contains(column))
                return false;

            var comparer = column.GetComparison(direction);

            if (comparer is object)
            {
                Sort(comparer);
                foreach (var c in Columns)
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
            return new AnonymousSortableRows<TModel>(_itemsView, _comparer);
        }

        private ICell CreateCell(IRow<TModel> row, int columnIndex)
        {
            return Columns[columnIndex].CreateCell(row);
        }
    }
}
