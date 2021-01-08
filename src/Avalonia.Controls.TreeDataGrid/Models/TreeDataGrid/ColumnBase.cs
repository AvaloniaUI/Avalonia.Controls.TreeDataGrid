using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Base class for columns which select cell values from a model.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    /// <typeparam name="TModel">The value type.</typeparam>
    public abstract class ColumnBase<TModel, TValue> : NotifyingBase, IColumn<TModel>
    {
        private GridLength _width;
        private object? _header;
        private ListSortDirection? _sortDirection;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnBase{TModel, TValue}"/> class.
        /// </summary>
        /// <param name="header">The column header.</param>
        /// <param name="width">The column width.</param>
        public ColumnBase(object? header, GridLength width, Func<TModel, TValue> valueSelector)
        {
            _header = header;
            _width = width;
            ValueSelector = valueSelector;
        }

        /// <summary>
        /// Gets or sets the width of the column.
        /// </summary>
        public GridLength Width 
        {
            get => _width;
            set => RaiseAndSetIfChanged(ref _width, value);
        }

        /// <summary>
        /// Gets or sets the column header.
        /// </summary>
        public object? Header
        {
            get => _header;
            set => RaiseAndSetIfChanged(ref _header, value);
        }

        /// <summary>
        /// Gets or sets the sort direction indicator that will be displayed on the column.
        /// </summary>
        /// <remarks>
        /// Note that changing this property does not change the sorting of the data, it is only 
        /// used to display a sort direction indicator. To sort data according to a column use
        /// <see cref="ITreeDataGridSource.SortBy(IColumn, ListSortDirection)"/>.
        /// </remarks>
        public ListSortDirection? SortDirection
        {
            get => _sortDirection;
            set => RaiseAndSetIfChanged(ref _sortDirection, value);
        }

        /// <summary>
        /// Gets the function which selects the column value from the model.
        /// </summary>
        public Func<TModel, TValue> ValueSelector { get; }

        /// <summary>
        /// Creates a cell for this column on the specified row.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <returns>The cell.</returns>
        public abstract ICell CreateCell(IRow<TModel> row);

        public Comparison<TModel>? GetComparison(ListSortDirection direction)
        {
            return (x, y) =>
            {
                var a = ValueSelector(x);
                var b = ValueSelector(y);
                var r = Comparer<TValue>.Default.Compare(a, b);
                return direction == ListSortDirection.Descending ? -r : r;
            };
        }
    }
}
