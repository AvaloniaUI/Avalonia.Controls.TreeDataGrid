using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Represents a collection of rows in an <see cref="ITreeDataGridSource"/>.
    /// </summary>
    /// <remarks>
    /// Note that items retrieved from an <see cref="IRows"/> collection may be reused, so the
    /// <see cref="IRow"/> should be treated as valid only until the next item is retrieved from
    /// the collection.
    /// </remarks>
    public interface IRows : IReadOnlyList<IRow>, INotifyCollectionChanged
    {
        /// <summary>
        /// Gets the index and Y position of the row at the specified Y position, if it can be
        /// calculated.
        /// </summary>
        /// <param name="y">The Y position</param>
        /// <returns>
        /// A tuple containing the row index and Y position of the row, or (-1,-1) if the row
        /// could not be calculated.
        /// </returns>
        (int index, double y) GetRowAt(double y);

        ICell RealizeCell(IColumn column, int columnIndex, int rowIndex);

        void UnrealizeCell(ICell cell, int columnIndex, int rowIndex);
    }
}
