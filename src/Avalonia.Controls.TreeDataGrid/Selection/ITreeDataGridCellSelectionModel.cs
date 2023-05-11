using System;
using System.Collections.Generic;
using Avalonia.Controls.Models.TreeDataGrid;

namespace Avalonia.Controls.Selection
{
    public interface ITreeDataGridCellSelectionModel : ITreeDataGridSelection
    {
    }

    public interface ITreeDataGridCellSelectionModel<T> : ITreeDataGridCellSelectionModel
        where T : class
    {
        /// <summary>
        /// Gets or sets a value indicating whether only a single cell can be selected at a time.
        /// </summary>
        bool SingleSelect { get; set; }

        /// <summary>
        /// Gets the currently selected cells.
        /// </summary>
        IReadOnlyList<ICell> SelectedCells { get; }

        /// <summary>
        /// Gets the currently selected columns.
        /// </summary>
        ITreeDataGridColumnSelectionModel SelectedColumns { get; }

        /// <summary>
        /// Gets the currently selected rows.
        /// </summary>
        ITreeDataGridRowSelectionModel<T> SelectedRows { get; }
    }
}
