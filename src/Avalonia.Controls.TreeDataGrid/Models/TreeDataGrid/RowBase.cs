namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Base class for rows with a model taken from a indexable data source.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public abstract class RowBase<TModel> : IRow
    {
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

        /// <summary>
        /// Gets the index of the model in the data source.
        /// </summary>
        public abstract int ModelIndex { get; }

        /// <summary>
        /// Updates the model index due to a change in the data source.
        /// </summary>
        /// <param name="delta">The index delta.</param>
        public abstract void UpdateModelIndex(int delta);
    }
}
