using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Avalonia.Controls
{
    /// <summary>
    /// Represents a data source for a <see cref="TreeDataGrid"/> control.
    /// </summary>
    public interface ITreeDataGridSource
    {
        /// <summary>
        /// Event which would be triggered after SortBy method execution.
        /// </summary>
        event Action Sorted;

        /// <summary>
        /// Gets the columns to be displayed.
        /// </summary>
        IColumns Columns { get; }

        /// <summary>
        /// Gets the rows to be displayed.
        /// </summary>
        IRows Rows { get; }

        /// <summary>
        /// Gets the selection model.
        /// </summary>
        ITreeDataGridSelection? Selection { get; }

        /// <summary>
        /// Requests to sort the source by the specified column.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="direction">The sort direction.</param>
        /// <returns>True if the sort could be performed; otherwise false.</returns>
        bool SortBy(IColumn column, ListSortDirection direction);
    }

    /// <summary>
    /// Represents a typed data source for a <see cref="TreeDataGrid"/> control.
    /// </summary>
    public interface ITreeDataGridSource<TModel> : ITreeDataGridSource
    {
        /// <summary>
        /// Gets the items in the data source.
        /// </summary>
        IEnumerable<TModel> Items { get; }
    }
}
