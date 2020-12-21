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
        /// Temorary hack for selection purposes: returns the model for a row index.
        /// </summary>
        /// <param name="rowIndex">The row index.</param>
        object? RowToModelHack(int rowIndex);

        /// <summary>
        /// Temorary hack for selection purposes: returns the row index for a model.
        /// </summary>
        /// <param name="model">The model.</param>
        int ModelToRowHack(object? model);
    }
}
