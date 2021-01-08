using System;
using System.Collections.Generic;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// A column in an <see cref="HierarchicalTreeDataGridSource{TModel}"/> whose cells show an
    /// expander to reveal nested data.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    /// <typeparam name="TValue">The column data type.</typeparam>
    public class HierarchicalExpanderColumn<TModel, TValue> : ColumnBase<TModel, TValue>,
        IExpanderColumn<TModel>
    {
        private readonly Func<TModel, IEnumerable<TModel>?> _childSelector;
        private readonly Func<TModel, bool>? _hasChildrenSelector;

        /// <summary>
        /// Initializes a new instance of the <see cref="HierarchicalExpanderColumn{TModel}"/> class.
        /// </summary>
        /// <param name="header">The column header.</param>
        /// <param name="valueSelector">The cell value selector.</param>
        /// <param name="childSelector">The model children selector.</param>
        /// <param name="hasChildrenSelector">The has model children selector.</param>
        /// <param name="width">The column width.</param>
        public HierarchicalExpanderColumn(
            object? header,
            Func<TModel, TValue> valueSelector,
            Func<TModel, IEnumerable<TModel>?> childSelector,
            Func<TModel, bool>? hasChildrenSelector,
            GridLength width)
            : base(header, width, valueSelector)
        {
            _childSelector = childSelector;
            _hasChildrenSelector = hasChildrenSelector;
        }

        public override ICell CreateCell(IRow<TModel> row)
        {
            if (row is HierarchicalRow<TModel> r)
            {
                return new ExpanderCell<TModel, TValue>(
                    this,
                    r,
                    ValueSelector(r.Model),
                    _hasChildrenSelector?.Invoke(row.Model) ?? true);
            }

            throw new NotSupportedException();
        }

        public IEnumerable<TModel>? GetChildModels(TModel model) => _childSelector(model);
    }
}
