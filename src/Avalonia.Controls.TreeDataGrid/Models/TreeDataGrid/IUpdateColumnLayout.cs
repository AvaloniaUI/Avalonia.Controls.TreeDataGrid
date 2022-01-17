namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Internal low-level interface for layout interactions between <see cref="IColumns"/> and
    /// <see cref="IColumn"/>.
    /// </summary>
    public interface IUpdateColumnLayout : IColumn
    {
        /// <summary>
        /// Notifies the column that a cell has been measured.
        /// </summary>
        /// <param name="width">The measured width, in pixels; as returned by DesiredSize.</param>
        /// <param name="rowIndex">The cell row index or -1 for a column header.</param>
        /// <returns>
        /// The width of the cell updated with the column width.
        /// </returns>
        double CellMeasured(double width, int rowIndex);

        /// <summary>
        /// Requests a non-star-width column to set its final <see cref="IColumn.ActualWidth"/>.
        /// </summary>
        void CommitActualWidth();

        /// <summary>
        /// Requests a star-width column to set its final <see cref="IColumn.ActualWidth"/>.
        /// </summary>
        /// <param name="availableWidth">
        /// The available width to be shared by star-width columns.
        /// </param>
        /// <param name="totalStars">The sum of the star units of all columns.</param>
        void CommitActualWidth(double availableWidth, double totalStars);

        /// <summary>
        /// Notifies the column of a change to its preferred width.
        /// </summary>
        /// <param name="width">The width.</param>
        void SetWidth(GridLength width);
    }
}
