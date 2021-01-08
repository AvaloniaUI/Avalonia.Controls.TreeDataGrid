using System.Collections.Generic;
using System.Collections.Specialized;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Represents a collection of cells in an <see cref="ITreeDataGridSource"/>.
    /// </summary>
    /// <remarks>
    /// Note that items retrieved from an <see cref="ICells"/> collection may be reused, so the
    /// <see cref="ICell"/> should be treated as valid only until the next item is retrieved from
    /// the collection.
    /// </remarks>
    public interface ICells : IReadOnlyList<ICell>, INotifyCollectionChanged
    {
        /// <summary>
        /// Gets the number of columns.
        /// </summary>
        int ColumnCount { get; }

        /// <summary>
        /// Gets the number of rows.
        /// </summary>
        int RowCount { get; }

        /// <summary>
        /// Gets the cell at the specified coordinates.
        /// </summary>
        /// <param name="column">The column index.</param>
        /// <param name="row">The row index.</param>
        /// <returns></returns>
        ICell this[int column, int row] { get; }
    }
}
