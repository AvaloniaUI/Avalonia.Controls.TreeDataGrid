using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using System;
using System.ComponentModel;

namespace Avalonia.Controls
{
    /// <summary>
    /// Represents a data source for a <see cref="TreeDataGrid"/> control.
    /// </summary>
    public interface ITreeDataGridSource
    {
        /// <summary>
        /// Raised before the source begins a sort operation.
        /// </summary>
        event Action BeforeSort;

        /// <summary>
        /// Raised after a sort operation on the the source ends.
        /// </summary>
        event Action AfterSort;

        /// <summary>
        /// Gets the columns to be displayed.
        /// </summary>
        IColumns Columns { get; }

        /// <summary>
        /// Gets the rows to be displayed.
        /// </summary>
        IRows Rows { get; }

        /// <summary>
        /// Requests to sort the source by the specified column.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="direction">The sort direction.</param>
        /// <param name="selection">Selection model to keep selection during sorting.</param>
        /// <returns>True if the sort could be performed; otherwise false.</returns>
        bool SortBy(IColumn column, ListSortDirection direction, ISelectionModel selection);
    }
}
