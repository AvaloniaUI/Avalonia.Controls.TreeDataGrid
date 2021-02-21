using System;
using System.ComponentModel;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Base class for columns which select cell values from a model.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public abstract class ColumnBase<TModel> : NotifyingBase, IColumn<TModel>
    {
        private bool? _canUserResize;
        private GridLength _width;
        private object? _header;
        private ListSortDirection? _sortDirection;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnBase{TModel, TValue}"/> class.
        /// </summary>
        /// <param name="header">The column header.</param>
        /// <param name="width">
        /// The column width. If null defaults to <see cref="GridLength.Auto"/>.
        /// </param>
        /// <param name="options">Additional column options.</param>
        public ColumnBase(
            object? header,
            GridLength? width,
            ColumnOptions<TModel>? options)
        {
            _canUserResize = options?.CanUserResizeColumn;
            _header = header;
            _width = width ?? GridLength.Auto;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user can resize the column.
        /// </summary>
        public bool? CanUserResize
        {
            get => _canUserResize;
            set => RaiseAndSetIfChanged(ref _canUserResize, value);
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
        /// Creates a cell for this column on the specified row.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <returns>The cell.</returns>
        public abstract ICell CreateCell(IRow<TModel> row);

        public abstract Comparison<TModel>? GetComparison(ListSortDirection direction);
    }
}
