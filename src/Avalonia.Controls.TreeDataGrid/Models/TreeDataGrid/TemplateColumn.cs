using System;
using System.ComponentModel;
using Avalonia.Controls.Templates;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// A column in an <see cref="ITreeDataGridSource"/> which displays its values using a data
    /// template.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    /// <typeparam name="TValue">The column data type.</typeparam>
    public class TemplateColumn<TModel> : NotifyingBase, IColumn<TModel>
    {
        private readonly Comparison<TModel>? _sortAscending;
        private readonly Comparison<TModel>? _sortDescending;
        private GridLength _width;
        private object? _header;
        private ListSortDirection? _sortDirection;

        public TemplateColumn(
            object? header,
            IDataTemplate cellTemplate,
            GridLength width,
            ColumnOptions<TModel>? options = null)
        {
            _header = header;
            _width = width;
            _sortAscending = options?.CompareAscending;
            _sortDescending = options?.CompareDescending;
            CellTemplate = cellTemplate;
        }

        /// <summary>
        /// Gets the template to use to display the contents of a cell that is not in editing mode.
        /// </summary>
        public IDataTemplate? CellTemplate { get; }

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
        public ICell CreateCell(IRow<TModel> row) => new TemplateCell(row.Model, CellTemplate);

        public Comparison<TModel>? GetComparison(ListSortDirection direction)
        {
            return direction switch
            {
                ListSortDirection.Ascending => _sortAscending,
                ListSortDirection.Descending => _sortDescending,
                _ => null,
            };
        }
    }
}
