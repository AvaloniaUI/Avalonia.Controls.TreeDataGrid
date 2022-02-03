using System.Collections.Generic;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Defines a column whose cells show an expander to reveal nested data.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public interface IExpanderColumn<TModel> : IColumn<TModel>
    {
        /// <summary>
        /// Gets a value indicating whether the column has nested data.
        /// </summary>
        /// <param name="model">The parent model.</param>
        bool HasChildren(TModel model);

        /// <summary>
        /// Gets the child models which represent the nested data for this column.
        /// </summary>
        /// <param name="model">The parent model.</param>
        /// <returns>The child models if available.</returns>
        IEnumerable<TModel>? GetChildModels(TModel model);

        /// <summary>
        /// Gets a value indicating whether the model is marked as expanded.
        /// </summary>
        /// <param name="model">The model.</param>
        bool IsExpanded(TModel model);
    }
}
