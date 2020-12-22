namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Base class for columns with a model type.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public abstract class ColumnBase<TModel> : IColumn
    {
        /// <summary>
        /// Gets or sets the width of the column.
        /// </summary>
        public abstract GridLength Width { get; set; }

        /// <summary>
        /// Gets the column header.
        /// </summary>
        public abstract object? Header { get; }
    }
}
