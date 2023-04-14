using Avalonia.Media;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    public interface ITextCellOptions
    {
        /// <summary>
        /// Gets a value indicating whether a single tap will begin edit mode on a cell.
        /// </summary>
        /// <remarks>
        /// If false, a double-tap is required to enter edit mode.
        /// </remarks>
        bool SingleTapEdit { get; }

        /// <summary>
        /// Gets the text trimming mode for the cell.
        /// </summary>
        TextTrimming TextTrimming { get; }

        /// <summary>
        /// Gets the text alignment mode for the cell.
        /// </summary>
        TextAlignment TextAlignment { get; }
    }
}
