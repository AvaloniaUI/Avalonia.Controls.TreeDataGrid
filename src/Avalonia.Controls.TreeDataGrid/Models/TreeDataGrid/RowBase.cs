namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Base class for rows with a model type.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public abstract class RowBase<TModel> : IRow
    {
        /// <summary>
        /// Gets the row index.
        /// </summary>
        public abstract int Index { get; }

        /// <summary>
        /// Gets or sets the height of the row.
        /// </summary>
        public abstract GridLength Height { get; set; }

        /// <summary>
        /// Gets the row header.
        /// </summary>
        public abstract object? Header { get; }

        /// <summary>
        /// Gets the row model.
        /// </summary>
        public abstract TModel Model { get; }
    }
}
