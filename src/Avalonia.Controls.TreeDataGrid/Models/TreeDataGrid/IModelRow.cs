namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Base interface for rows with a model taken from an indexable data source.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public interface IModelRow : IRow
    {
        /// <summary>
        /// Gets the row model.
        /// </summary>
        object Model { get; }

        /// <summary>
        /// Gets the index of the model in the data source.
        /// </summary>
        int ModelIndex { get; }

        /// <summary>
        /// Updates the model index due to a change in the data source.
        /// </summary>
        /// <param name="delta">The index delta.</param>
        void UpdateModelIndex(int delta);
    }
}
