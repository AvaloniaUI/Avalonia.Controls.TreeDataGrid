using System;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// A column in an <see cref="HierarchicalTreeDataGridSource{TModel}"/> which displays its values as text.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    /// <typeparam name="TValue">The column data type.</typeparam>
    public class TextColumn<TModel, TValue> : ColumnBase<TModel, TValue>
    {
        public TextColumn(
            object? header,
            GridLength width,
            Func<TModel, TValue> valueSelector,
            ColumnOptions<TModel>? options = null)
            : base(header, width, valueSelector, options)
        {
        }

        public override ICell CreateCell(IRow<TModel> row) => new TextCell<TValue>(ValueSelector(row.Model));
    }
}
