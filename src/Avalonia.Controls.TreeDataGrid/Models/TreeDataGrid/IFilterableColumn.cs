namespace Avalonia.Controls.Models.TreeDataGrid
{
    public interface IFilterableColumn
    {
        
        /// <summary>
        /// Gets a value indicating whether filtering is enabled for this column.
        /// </summary>
        bool IsFilterEnabled { get; }
    }
    /// <summary>
    /// Interface for columns that support text-based filtering.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public interface IFilterableColumn<TModel>: IFilterableColumn
    {
        bool PassesFilter(TModel model, object? condition);
    }
}
