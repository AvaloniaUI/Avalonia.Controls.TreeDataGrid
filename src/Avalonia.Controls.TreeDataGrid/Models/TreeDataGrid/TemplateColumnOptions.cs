using System;
using Avalonia.Controls.Templates;
using Avalonia.Media;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Holds less commonly-used options for a <see cref="TemplateColumn{TModel}"/>.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public class TemplateColumnOptions<TModel> : ColumnOptions<TModel>, ITemplateCellOptions, IFilterControlFactory
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
        /// Gets or sets a function which selects the search text from a model.
        /// </summary>
        public Func<TModel, string?>? TextSearchValueSelector { get; set; }

        /// <summary>
        /// Gets or sets a function which selects the filter value from a model.
        /// If null, TextSearchValueSelector will be used for filtering.
        /// </summary>
        public Func<TModel, object?>? FilterValueSelector { get; set; }

        /// <summary>
        /// Gets or sets the filter to use for this column.
        /// </summary>
        public IValueFilter? Filter { get; set; }


        /// <summary>
        /// Gets or sets a custom filter control factory to use for creating the filter control.
        /// If null, a default text filter will be used.
        /// </summary>
        public IFilterControlFactory? CustomFilterFactory { get; set; }

        /// <summary>
        /// Creates a filter control for this column.
        /// </summary>
        /// <param name="column">The column for which to create a filter control.</param>
        /// <param name="initialValue">The initial filter value.</param>
        /// <returns>A filter control that can be used to filter the column.</returns>
        public IFilterControl? CreateFilterControl(IColumn column, object? initialValue)
        {
            if (column is not TemplateColumn<TModel> || !IsFilterEnabled) return null;
            // Use the custom factory if one is provided
            return CustomFilterFactory?.CreateFilterControl(column, initialValue);
        }
    }
}
