using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Represents a collection of columns in an <see cref="ITreeDataGridSource"/>.
    /// </summary>
    /// <remarks>
    /// Note that items retrieved from an <see cref="IColumns"/> collection may be reused, so the
    /// <see cref="IColumn"/> should be treated as valid only until the next item is retrieved from
    /// the collection.
    /// </remarks>
    public interface IColumns : IReadOnlyList<IColumn>, INotifyCollectionChanged
    {
        event EventHandler LayoutInvalidated;

        /// <summary>
        /// Called by the <see cref="TreeDataGrid"/> when a cell has been measured.
        /// </summary>
        /// <param name="columnIndex">The cell column index or -1 for a row header.</param>
        /// <param name="rowIndex">The cell row index or -1 for a column header.</param>
        /// <param name="size">The measured size.</param>
        void CellMeasured(int columnIndex, int rowIndex, Size size);

        /// <summary>
        /// Gets the index and X position of the column at the specified X position, if it can be
        /// calculated.
        /// </summary>
        /// <param name="x">The X position</param>
        /// <returns>
        /// A tuple containing the column index and X position of the column, or (-1,-1) if the 
        /// column could not be calculated.
        /// </returns>
        (int index, double x) GetColumnAt(double x);

        /// <summary>
        /// Called by the <see cref="TreeDataGrid"/> when a measure pass has completed.
        /// </summary>
        void MeasureFinished();

        /// <summary>
        /// Sets the width of a column.
        /// </summary>
        /// <param name="columnIndex">The column index.</param>
        /// <param name="width">The column width</param>
        void SetColumnWidth(int columnIndex, GridLength width);

        /// <summary>
        /// Called by the <see cref="TreeDataGrid"/> when the viewport changes, in order to update
        /// star columns.
        /// </summary>
        /// <param name="viewport">The current viewport.</param>
        void ViewportChanged(Rect viewport);
    }
}
