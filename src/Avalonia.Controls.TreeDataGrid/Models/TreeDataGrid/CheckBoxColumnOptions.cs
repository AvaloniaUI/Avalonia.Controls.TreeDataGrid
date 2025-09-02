using System;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Holds less commonly-used options for a <see cref="CheckBoxColumn{TModel}"/>.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public class CheckBoxColumnOptions<TModel> : ColumnOptions<TModel>
    {
        /// <summary>
        /// Gets or sets a value indicating whether filtering is enabled for this column.
        /// </summary>
        public bool IsFilterEnabled { get; set; }
    }
}
