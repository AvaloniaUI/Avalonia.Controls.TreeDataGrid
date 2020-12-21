﻿namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Base class for columns in an <see cref="ITreeDataGridSource"/> which use a model as their data
    /// source.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public abstract class ColumnBase<TModel> : IColumn
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnBase{TModel}"/> class.
        /// </summary>
        /// <param name="header">The column header.</param>
        /// <param name="width">The column width.</param>
        protected ColumnBase(object? header, GridLength width)
        {
            Header = header;
            Width = width;
        }

        /// <summary>
        /// Gets or sets the width of the column.
        /// </summary>
        public GridLength Width { get; set; }

        /// <summary>
        /// Gets the column header.
        /// </summary>
        public object? Header { get; }
    }
}
