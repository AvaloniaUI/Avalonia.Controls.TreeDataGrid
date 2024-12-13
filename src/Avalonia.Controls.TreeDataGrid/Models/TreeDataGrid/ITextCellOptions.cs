using System.Globalization;

using Avalonia.Media;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    public interface ITextCellOptions : ICellOptions
    {
        /// <summary>
        /// Gets the format string to be used to format the cell value.
        /// </summary>
        string StringFormat { get; }

        /// <summary>
        /// Gets the culture to be used in conjunction with <see cref="StringFormat"/>.
        /// </summary>
        CultureInfo Culture { get; }

        /// <summary>
        /// Gets the text trimming mode for the cell.
        /// </summary>
        TextTrimming TextTrimming { get; }

        /// <summary>
        /// Gets the text wrapping mode for the cells in the column.
        /// </summary>
        TextWrapping TextWrapping { get; }
        
        /// Gets the text alignment mode for the cell.
        /// </summary>
        TextAlignment TextAlignment { get; }
    }
}
