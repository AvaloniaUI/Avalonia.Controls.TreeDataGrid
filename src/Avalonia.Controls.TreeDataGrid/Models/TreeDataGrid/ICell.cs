using System;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Represents a cell in an <see cref="ITreeDataGridSource"/>.
    /// </summary>
    public interface ICell
    {
        /// <summary>
        /// Gets a value indicating whether the cell can enter edit mode.
        /// </summary>
        bool CanEdit { get; }

        /// <summary>
        /// Gets a value indicating whether a single tap will begin edit mode on a cell.
        /// </summary>
        /// <remarks>
        /// If false, a double-tap is required to enter edit mode.
        /// </remarks>
        bool SingleTapEdit { get; }

        /// <summary>
        /// Gets the value of the cell.
        /// </summary>
        object? Value { get; }
    }
}
