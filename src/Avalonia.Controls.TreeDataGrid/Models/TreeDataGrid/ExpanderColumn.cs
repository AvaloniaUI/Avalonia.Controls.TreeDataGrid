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
    public class ExpanderColumn<TModel, TValue> : ExpanderColumnBase<TModel>
    {
        private readonly Func<TModel, TValue> _valueSelector;
        private readonly Func<TModel, IEnumerable<TModel>?> _childSelector;
        private readonly Func<TModel, bool>? _hasChildrenSelector;

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeListExpanderColumn{TModel}"/> class.
        /// </summary>
        /// <param name="header">The column header.</param>
        /// <param name="valueSelector">The cell value selector.</param>
        /// <param name="width">The column width.</param>
        public ExpanderColumn(
            IExpanderController owner,
            object? header,
            Func<TModel, TValue> valueSelector,
            Func<TModel, IEnumerable<TModel>?> childSelector,
            GridLength width,
            Func<TModel, bool>? hasChildrenSelector = null)
            : base(owner, header, width)
        {
            _valueSelector = valueSelector;
            _childSelector = childSelector;
            _hasChildrenSelector = hasChildrenSelector;
        }

        public override ICell CreateCell(IndexPath index, TModel model)
        {
            return new ExpanderCell<TModel, TValue>(
                this,
                model,
                index,
                _valueSelector(model),
                _hasChildrenSelector?.Invoke(model) ?? true);
        }

        public override IEnumerable<TModel>? GetChildModels(TModel model) => _childSelector(model);
    }
}
