using System;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Represents a column in an <see cref="ITreeDataGridSource"/>.
    /// </summary>
    public interface IColumn
    {
        /// <summary>
        /// Gets the column header.
        /// </summary>
        object? Header { get; }

        /// <summary>
        /// Gets the width of the column.
        /// </summary>
        GridLength Width { get; set; }
    }
}
