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

        public Func<IFilterableColumn, IFilterControl>? FilterControlFactory { get; set; }

        /// <summary>
        /// Creates a filter control for this column.
        /// </summary>
        /// <param name="column">The column for which to create a filter control.</param>
        /// <returns>A filter control that can be used to filter the column.</returns>
        public IFilterControl? CreateFilterControl(IColumn column)
        {
            if (Filter == null) return null;
            if (FilterControlFactory == null) return null;
            if (!(column is TemplateColumn<TModel> col)) return null;
            return FilterControlFactory?.Invoke(col);
        }
    }
}
