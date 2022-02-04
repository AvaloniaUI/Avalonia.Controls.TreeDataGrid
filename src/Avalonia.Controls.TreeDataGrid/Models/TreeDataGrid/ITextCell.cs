using System;
using Avalonia.Media;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Represents a text cell in an <see cref="ITreeDataGridSource"/>.
    /// </summary>
    public interface ITextCell : ICell
    {
        /// <summary>
        /// Gets the cell's text trimming mode.
        /// </summary>
        TextTrimming TextTrimming { get; }
    }
}
