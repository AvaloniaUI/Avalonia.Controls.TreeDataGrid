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
    public class TemplateColumn<TModel> : ColumnBase<TModel>
    {
        private readonly Comparison<TModel?>? _sortAscending;
        private readonly Comparison<TModel?>? _sortDescending;

        public TemplateColumn(
            object? header,
            IDataTemplate cellTemplate,
            GridLength? width = null,
            ColumnOptions<TModel>? options = null)
            : base(header, width, options)
        {
            _sortAscending = options?.CompareAscending;
            _sortDescending = options?.CompareDescending;
            CellTemplate = cellTemplate;
        }

        /// <summary>
        /// Gets the template to use to display the contents of a cell that is not in editing mode.
        /// </summary>
        public IDataTemplate? CellTemplate { get; }

        /// <summary>
        /// Creates a cell for this column on the specified row.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <returns>The cell.</returns>
        public override ICell CreateCell(IRow<TModel> row) => new TemplateCell(row.Model, CellTemplate);

        public override Comparison<TModel?>? GetComparison(ListSortDirection direction)
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
