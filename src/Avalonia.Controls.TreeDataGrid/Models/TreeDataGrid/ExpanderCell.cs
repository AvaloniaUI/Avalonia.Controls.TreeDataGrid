namespace Avalonia.Controls.Models.TreeDataGrid
{
    public class ExpanderCell<TModel, TValue> : ExpanderCellBase<TModel>
    {
        public ExpanderCell(
            ExpanderColumnBase<TModel> column,
            HierarchicalRow<TModel> row,
            TValue value,
            bool showExpander)
            : base(column, row, row.Model, row.Index, showExpander)
        {
            Value = value;
        }

        public TValue Value { get; }

        protected override object? GetUntypedValue() => Value;
    }
}
