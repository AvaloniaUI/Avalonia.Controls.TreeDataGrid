namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Represents a row in an <see cref="ITreeDataGridSource"/>.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public interface IRow<TModel> : IRow
    {
        /// <summary>
        /// Gets the row model.
        /// </summary>
        TModel Model { get; }

        /// <summary>
        /// Updates the model index due to a change in the data source.
        /// </summary>
        /// <param name="delta">The index delta.</param>
        void UpdateModelIndex(int delta);
    }
}
