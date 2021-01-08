namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Represents a row which can be expanded to reveal nested data.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public interface IExpanderRow<TModel> : IRow<TModel>, IExpander
    {
    }
}
