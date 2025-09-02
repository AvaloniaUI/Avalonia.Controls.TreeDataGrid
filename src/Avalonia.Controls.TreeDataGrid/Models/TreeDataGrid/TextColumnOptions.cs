using System.Globalization;

using Avalonia.Media;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Holds less commonly-used options for a <see cref="TextColumn{TModel, TValue}"/>.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public class TextColumnOptions<TModel> : ColumnOptions<TModel>, ITextCellOptions, IFilterControlFactory
    {
        /// <summary>
        /// Gets or sets a value indicating whether the column takes part in text searches.
        /// </summary>
        public bool IsTextSearchEnabled { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether filtering is enabled for this column.
        /// </summary>
        public bool IsFilterEnabled { get; set; }
        /// <summary>
        /// Gets or sets the format string for the cells in the column.
        /// </summary>
        public string StringFormat { get; set; } = "{0}";

        /// <summary>
        /// Culture info used in conjunction with <see cref="StringFormat"/>
        /// </summary>
        public CultureInfo Culture { get; set; } = CultureInfo.CurrentCulture;

        /// <summary>
        /// Gets or sets the text trimming mode for the cells in the column.
        /// </summary>
        public TextTrimming TextTrimming { get; set; } = TextTrimming.CharacterEllipsis;

        /// <summary>
        /// Gets or sets the text wrapping mode for the cells in the column.
        /// </summary>
        public TextWrapping TextWrapping { get; set; } = TextWrapping.NoWrap;
        
        
        /// Gets or sets the text alignment mode for the cells in the column.
        /// </summary>
        public TextAlignment TextAlignment { get; set; } = TextAlignment.Left;

        /// <summary>
        /// Gets or sets the filter to use for this column.
        /// </summary>
        public IValueFilter? Filter { get; set; } = new TextValueFilter();
        
        /// <summary>
        /// Gets or sets the filter prompt text to show in the filter box watermark.
        /// </summary>
        public string FilterPrompt { get; set; } = "Filter...";
        
        /// <summary>
        /// Creates a filter control for this column.
        /// </summary>
        /// <param name="column">The column for which to create a filter control.</param>
        /// <param name="initialValue">The initial filter value.</param>
        /// <returns>A filter control that can be used to filter the column.</returns>
        public IFilterControl? CreateFilterControl(IColumn column, object? initialValue)
        {
            if (column is IFilterableColumn<TModel> && IsFilterEnabled)
            {
                return new TextFilterControl(FilterPrompt, initialValue);
            }
            
            return null;
        }
    }
}
