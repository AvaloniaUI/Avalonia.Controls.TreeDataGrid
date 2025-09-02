﻿using System;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// Holds less commonly-used options for a <see cref="CheckBoxColumn{TModel}"/>.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public class CheckBoxColumnOptions<TModel> : ColumnOptions<TModel>, IFilterControlFactory
    where TModel : class
    {
        /// <summary>
        /// Gets or sets a value indicating whether filtering is enabled for this column.
        /// </summary>
        public bool IsFilterEnabled { get; set; }
        
        /// <summary>
        /// Gets or sets the filter to use for this column.
        /// </summary>
        public IValueFilter? Filter { get; set; } = new BooleanValueFilter();
        
        /// <summary>
        /// Gets or sets a value indicating whether the filter should use a three-state checkbox.
        /// </summary>
        public bool IsThreeStateFilter { get; set; } = true;
        
        /// <summary>
        /// Creates a filter control for this column.
        /// </summary>
        /// <param name="column">The column for which to create a filter control.</param>
        /// <param name="initialValue">The initial filter value.</param>
        /// <returns>A filter control that can be used to filter the column.</returns>
        public IFilterControl? CreateFilterControl(IColumn column, object? initialValue)
        {
            if (column is CheckBoxColumn<TModel> && IsFilterEnabled)
            {
                return new CheckBoxFilterControl(IsThreeStateFilter, initialValue);
            }
            
            return null;
        }
    }
}
