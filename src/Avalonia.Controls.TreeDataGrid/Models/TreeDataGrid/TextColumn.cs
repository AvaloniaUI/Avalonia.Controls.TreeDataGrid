using System;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    /// <summary>
    /// A column in an <see cref="HierarchicalTreeDataGridSource{TModel}"/> which displays its values as text.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    /// <typeparam name="TValue">The column data type.</typeparam>
    public class TextColumn<TModel, TValue> : SelectorColumnBase<TModel>
    {
        private readonly Func<TModel, TValue> _valueSelector;

        public TextColumn(
            object? header,
            Func<TModel, TValue> valueSelector,
            GridLength width)
            : base(header, width)
        {
            _valueSelector = valueSelector;
        }

        public override ICell CreateCell(TModel model)
        {
            return new TextCell<TValue>(_valueSelector(model));
        }
    }
}
