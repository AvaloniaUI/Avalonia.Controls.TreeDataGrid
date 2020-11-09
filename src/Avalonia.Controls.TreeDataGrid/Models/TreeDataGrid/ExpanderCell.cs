namespace Avalonia.Controls.Models.TreeDataGrid
{
    public class ExpanderCell<TModel, TValue> : ExpanderCellBase<TModel>
    {
        public ExpanderCell(
            ExpanderColumnBase<TModel> column,
            TModel model,
            IndexPath modelIndex,
            TValue value,
            bool showExpander)
            : base(column, model, modelIndex, showExpander)
        {
            Value = value;
        }

        public TValue Value { get; }

        protected override object? GetUntypedValue() => Value;
    }
}
