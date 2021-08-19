namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Base interface for rows with a strongly typed model taken from an indexable data source.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public interface IModelRow<TModel> : IModelRow
    {
        /// <summary>
        /// Gets the row model.
        /// </summary>
        new TModel Model { get; }
    }
}
