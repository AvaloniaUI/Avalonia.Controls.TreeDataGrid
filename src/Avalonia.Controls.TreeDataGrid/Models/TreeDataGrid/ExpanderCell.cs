namespace Avalonia.Controls.Models.TreeDataGrid
{
    public class ExpanderCell<TModel, TValue> : ExpanderCellBase<TModel>
    {
        public ExpanderCell(
            IExpanderColumn<TModel> column,
            IExpanderRow<TModel> row,
            TValue value,
            bool showExpander)
            : base(column, row, showExpander)
        {
            Value = value;
        }

        public TValue Value { get; }

        protected override object? GetUntypedValue() => Value;
    }
}
