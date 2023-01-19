using System;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Holds less commonly-used options for an <see cref="IColumn{TModel}"/>.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public class ColumnOptions<TModel>
    {
        /// <summary>
        /// Gets or sets a value indicating whether the user can resize a column by dragging.
        /// </summary>
        /// <remarks>
        /// If null, the owner <see cref="Avalonia.Controls.TreeDataGrid.CanUserResizeColumns"/>
        /// property value will apply.
        /// </remarks>
        public bool? CanUserResizeColumn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user can sort a column by clicking.
        /// </summary>
        /// <remarks>
        /// If null, the owner <see cref="Avalonia.Controls.TreeDataGrid.CanUserSortColumns"/>
        /// property value will apply.
        /// </remarks>
        public bool? CanUserSortColumn { get; set; }

        /// <summary>
        /// Gets or sets the minimum width for a column.
        /// </summary>
        public GridLength MinWidth { get; set; } = new GridLength(30, GridUnitType.Pixel);

        /// <summary>
        /// Gets or sets the maximum width for a column.
        /// </summary>
        public GridLength? MaxWidth { get; set; } = null;

        /// <summary>
        /// Gets or sets a custom comparison for ascending ordered columns.
        /// </summary>
        public Comparison<TModel?>? CompareAscending { get; set; }

        /// <summary>
        /// Gets or sets a custom comparison for descending ordered columns.
        /// </summary>
        public Comparison<TModel?>? CompareDescending { get; set; }

        /// <summary>
        /// Determines whether or not this column is visible.
        /// </summary>
        public bool IsVisible { get; set; } = true;
    }
}
