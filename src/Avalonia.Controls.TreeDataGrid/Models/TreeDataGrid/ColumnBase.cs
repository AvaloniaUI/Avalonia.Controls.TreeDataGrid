using System;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Base class for columns with a model type.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public abstract class ColumnBase<TModel> : IColumn
    {
        /// <summary>
        /// Gets or sets the width of the column.
        /// </summary>
        public abstract GridLength Width { get; set; }

        /// <summary>
        /// Gets the column header.
        /// </summary>
        public abstract object? Header { get; }

        /// <summary>
        /// Gets a comparer function for the column.
        /// </summary>
        /// <param name="descending">The sort direction.</param>
        /// <returns>
        /// The comparer function or null if comparing cannot be performed on the column.
        /// </returns>
        public virtual Func<TModel, TModel, int>? GetComparer(bool descending) => null;
    }
}
