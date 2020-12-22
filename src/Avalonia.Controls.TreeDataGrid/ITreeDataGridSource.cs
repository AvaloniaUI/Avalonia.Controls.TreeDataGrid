using Avalonia.Controls.Models.TreeDataGrid;

namespace Avalonia.Controls
{
    /// <summary>
    /// Represents a data source for a <see cref="TreeDataGrid"/> control.
    /// </summary>
    public interface ITreeDataGridSource
    {
        /// <summary>
        /// Gets the columns to be displayed.
        /// </summary>
        IColumns Columns { get; }

        /// <summary>
        /// Gets the rows to be displayed.
        /// </summary>
        IRows Rows { get; }

        /// <summary>
        /// Gets the cells to be displayed.
        /// </summary>
        ICells Cells { get; }

        /// <summary>
        /// Requests to sort the source by the specified column.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="descending">The sort direction.</param>
        /// <returns>True if the sort could be performed; otherwise false.</returns>
        bool SortBy(IColumn column, bool descending);
    }
}
